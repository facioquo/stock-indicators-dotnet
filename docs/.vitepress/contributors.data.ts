import { Octokit } from '@octokit/rest'
import { getGitHubToken } from './utils/github.js'

export interface Contributor {
  login: string
  avatar: string
  url: string
  contributions: number
}

declare const data: Contributor[]
export { data }

/**
 * Extract GitHub username from Co-authored-by tag
 * Format: Co-authored-by: Name <12345+username@users.noreply.github.com>
 * or: Co-authored-by: @username
 */
function extractCoAuthorUsername(line: string): string | null {
  // Match: <12345+username@users.noreply.github.com>
  const emailMatch = line.match(/<(?:\d+\+)?([^@]+)@users\.noreply\.github\.com>/)
  if (emailMatch) return emailMatch[1]

  // Match: `@username` (standalone, not inside an email address)
  const atMatch = line.match(/(?:^|[\s:])@([a-zA-Z0-9-]+)(?:\s|$)/)
  if (atMatch) return atMatch[1]

  return null
}

export default {
  async load(): Promise<Contributor[]> {
    try {
      const token = getGitHubToken({ verbose: false })

      const octokit = new Octokit({
        auth: token
      })

      // Get direct contributors
      const { data: directContributors } = await octokit.repos.listContributors({
        owner: 'facioquo',
        repo: 'stock-indicators-dotnet',
        per_page: 100
      })

      // Filter function to exclude bots
      const isBotUser = (login: string): boolean => {
        const lowerLogin = login.toLowerCase()
        return lowerLogin.includes('[bot]') ||
               lowerLogin.startsWith('claude') ||
               lowerLogin.startsWith('codex') ||
               lowerLogin.startsWith('copilot') ||
               lowerLogin.includes('dependabot') ||
               lowerLogin.includes('imgbot')
      }

      // Get co-authors from commit messages
      const coAuthors = new Map<string, any>() // username -> user data
      let page = 1
      let hasMore = true

      while (hasMore && page <= 10) { // Limit to 10 pages to avoid excessive API calls and build time
        const { data: commits } = await octokit.repos.listCommits({
          owner: 'facioquo',
          repo: 'stock-indicators-dotnet',
          per_page: 100,
          page
        })

        if (commits.length === 0) {
          hasMore = false
          break
        }

        for (const commit of commits) {
          const message = commit.commit.message
          const coAuthorLines = message.match(/^Co-authored-by:.+$/gim)

          if (coAuthorLines) {
            for (const line of coAuthorLines) {
              const username = extractCoAuthorUsername(line)
              // Skip bots, invalid usernames, and already-processed users
              const skipUsernames = ['gmail', 'users'] // Known invalid usernames
              if (username && !isBotUser(username) && !skipUsernames.includes(username) && !coAuthors.has(username)) {
                try {
                  const { data: user } = await octokit.users.getByUsername({ username })
                  coAuthors.set(username, {
                    login: user.login,
                    avatar_url: user.avatar_url,
                    html_url: user.html_url,
                    contributions: 0
                  })
                } catch (error) {
                  // Silently skip users that can't be fetched
                }
              }
            }
          }
        }

        page++
      }

      // Combine contributors
      const contributorMap = new Map<string, Contributor>()

      // Add direct contributors (excluding bots)
      for (const contributor of directContributors) {
        if (contributor.login && !isBotUser(contributor.login)) {
          contributorMap.set(contributor.login, {
            login: contributor.login,
            avatar: `${contributor.avatar_url}&s=75`,
            url: contributor.html_url || '#',
            contributions: contributor.contributions || 0
          })
        }
      }

      // Add co-authors (if not already in the list)
      for (const [username, userData] of coAuthors) {
        if (!contributorMap.has(username)) {
          contributorMap.set(username, {
            login: userData.login,
            avatar: `${userData.avatar_url}&s=75`,
            url: userData.html_url || '#',
            contributions: 0
          })
        }
      }

      // Convert to array and sort by contributions
      return Array.from(contributorMap.values())
        .sort((a, b) => b.contributions - a.contributions)
    } catch (error) {
      console.error('Error fetching contributors:', error)
      return []
    }
  }
}
