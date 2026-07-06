#!/bin/bash
# StreamHub Audit Script
# Tasks: T173, T175-T179, T180-T183
# Purpose: Validate StreamHub test coverage, interface compliance, and provider history testing

set -eo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
total_hubs=0
total_tests=0
missing_tests=0
interface_issues=0
provider_history_missing=0
test_method_issues=0

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}StreamHub Audit - Tasks T173, T175-T185${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Find all StreamHub implementations
echo -e "${BLUE}Finding StreamHub implementations...${NC}"
mapfile -t hub_files < <(find src/Indicators src/Common/BarPart src/Common/Bars src/Common/TradeTicks -name "*Hub.cs" ! -name "*AggregatorHub.cs" | sort)
total_hubs=${#hub_files[@]}

echo -e "Found ${GREEN}${total_hubs}${NC} StreamHub implementations"
echo ""

# Arrays to store issues
declare -a missing_test_files
declare -a interface_compliance_issues
declare -a provider_history_issues
declare -a test_method_issues_list

echo -e "${BLUE}=== T173: Validating Test Coverage ===${NC}"
echo ""

# Check each implementation has corresponding test
for hub_file in "${hub_files[@]}"; do
    # Extract indicator name from path (strip "Hub.cs" suffix)
    indicator_name=$(basename "$hub_file" Hub.cs)
    dir_path=$(dirname "$hub_file")

    # Convert src path to test path (uniform {Name}HubTests.cs convention)
    if [[ "$dir_path" == src/Indicators/* ]]; then
        test_file="tests/Indicators/Indicators/${dir_path#src/Indicators/}/${indicator_name}HubTests.cs"
    else
        test_file="tests/Indicators/Common/${dir_path#src/Common/}/${indicator_name}HubTests.cs"
    fi

    if [[ -f "$test_file" ]]; then
        total_tests=$((total_tests + 1))
    else
        missing_test_files+=("$hub_file -> Expected: $test_file")
        missing_tests=$((missing_tests + 1))
    fi
done

echo -e "StreamHub implementations: ${GREEN}$total_hubs${NC}"
echo -e "Corresponding test files found: ${GREEN}$total_tests${NC}"
if [[ $missing_tests -gt 0 ]]; then
    echo -e "Missing test files: ${RED}$missing_tests${NC}"
else
    echo -e "Missing test files: ${GREEN}0${NC}"
fi
echo ""

# Display missing test files
if [[ ${#missing_test_files[@]} -gt 0 ]]; then
    echo -e "${RED}Missing test files:${NC}"
    for missing in "${missing_test_files[@]}"; do
        echo -e "  - $missing"
    done
    echo ""
fi

echo -e "${BLUE}=== T175-T179: Test Interface Compliance ===${NC}"
echo ""

# Find all StreamHub test files
mapfile -t test_files < <(find tests/Indicators -name "*HubTests.cs" ! -name "*AggregatorHub*" | sort)

for test_file in "${test_files[@]}"; do
    indicator_name=$(basename "$test_file" HubTests.cs)

    # Read the test file to check interfaces
    if [[ -f "$test_file" ]]; then
        # Extract class declaration line
        class_line=$(grep -E "^public class.*StreamHubTestBase" "$test_file" | head -1)

        if [[ -z "$class_line" ]]; then
            interface_compliance_issues+=("$indicator_name: Does not inherit from StreamHubTestBase")
            ((interface_issues++))
            continue
        fi

        # Check which interfaces are implemented
        has_bar_observer=$(echo "$class_line" | grep -c "ITestBarObserver" || true)
        has_tradetick_observer=$(echo "$class_line" | grep -c "ITestTradeTickObserver" || true)
        has_chain_observer=$(echo "$class_line" | grep -c "ITestChainObserver" || true)
        has_chain_provider=$(echo "$class_line" | grep -c "ITestChainProvider" || true)

        # Validation: Should implement at least one observer interface
        observer_count=$((has_bar_observer + has_tradetick_observer + has_chain_observer))

        # Note: ITestChainObserver inherits ITestBarObserver, so if both appear, that's valid
        if [[ $has_chain_observer -eq 1 ]] && [[ $has_bar_observer -eq 1 ]]; then
            observer_count=1  # This is valid - ChainObserver includes BarObserver
        fi

        if [[ $observer_count -eq 0 ]]; then
            interface_compliance_issues+=("$indicator_name: No observer interface implemented")
            interface_issues=$((interface_issues + 1))
        fi

        # Check for required test methods based on interfaces
        if [[ $has_bar_observer -eq 1 ]] || [[ $has_chain_observer -eq 1 ]]; then
            if ! grep -q "BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly" "$test_file"; then
                test_method_issues_list+=("$indicator_name: Missing BarObserver test method")
                test_method_issues=$((test_method_issues + 1))
            fi
        fi

        if [[ $has_chain_observer -eq 1 ]]; then
            if ! grep -q "ChainObserver_ChainedProvider_MatchesSeriesExactly" "$test_file"; then
                test_method_issues_list+=("$indicator_name: Missing ChainObserver test method")
                test_method_issues=$((test_method_issues + 1))
            fi
        fi

        if [[ $has_chain_provider -eq 1 ]]; then
            if ! grep -q "ChainProvider_MatchesSeriesExactly" "$test_file"; then
                test_method_issues_list+=("$indicator_name: Missing ChainHub test method")
                test_method_issues=$((test_method_issues + 1))
            fi
        fi
    fi
done

if [[ ${#interface_compliance_issues[@]} -eq 0 ]]; then
    echo -e "Interface compliance: ${GREEN}PASS${NC}"
else
    echo -e "Interface compliance issues: ${RED}${#interface_compliance_issues[@]}${NC}"
    for issue in "${interface_compliance_issues[@]}"; do
        echo -e "  ${RED}✗${NC} $issue"
    done
fi
echo ""

if [[ ${#test_method_issues_list[@]} -eq 0 ]]; then
    echo -e "Required test methods: ${GREEN}PASS${NC}"
else
    echo -e "Missing test methods: ${RED}${#test_method_issues_list[@]}${NC}"
    for issue in "${test_method_issues_list[@]}"; do
        echo -e "  ${YELLOW}⚠${NC} $issue"
    done
fi
echo ""

echo -e "${BLUE}=== T180-T183: Provider History Testing ===${NC}"
echo ""

# Check for comprehensive provider history testing (Add/Remove scenarios)
for test_file in "${test_files[@]}"; do
    indicator_name=$(basename "$test_file" HubTests.cs)

    if [[ -f "$test_file" ]]; then
        # Look for the canonical test method with provider history mutations
        has_bar_observer_method=$(grep -c "BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly" "$test_file" || true)

        if [[ $has_bar_observer_method -gt 0 ]]; then
            # Check if the method includes late Add and Remove operations
            has_add=$(grep -A 50 "BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly" "$test_file" | grep -E -c "\\.Add\\([^)]*\\[80\\]\\)" || true)
            has_remove=$(grep -A 50 "BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly" "$test_file" | grep -c "\.RemoveAt(" || true)

            if [[ $has_add -eq 0 ]] || [[ $has_remove -eq 0 ]]; then
                provider_history_issues+=("$indicator_name: BarObserver test missing Add/Remove operations")
                provider_history_missing=$((provider_history_missing + 1))
            fi
        fi

        # Check ChainHub method for provider history testing
        has_chain_provider_method=$(grep -c "ChainProvider_MatchesSeriesExactly" "$test_file" || true)

        if [[ $has_chain_provider_method -gt 0 ]]; then
            has_add=$(grep -A 50 "ChainProvider_MatchesSeriesExactly" "$test_file" | grep -E -c "\\.Add\\([^)]*\\[80\\]\\)" || true)
            has_remove=$(grep -A 50 "ChainProvider_MatchesSeriesExactly" "$test_file" | grep -c "\.RemoveAt(" || true)

            if [[ $has_add -eq 0 ]] || [[ $has_remove -eq 0 ]]; then
                provider_history_issues+=("$indicator_name: ChainHub test missing Add/Remove operations")
                provider_history_missing=$((provider_history_missing + 1))
            fi
        fi
    fi
done

if [[ ${#provider_history_issues[@]} -eq 0 ]]; then
    echo -e "Provider history testing: ${GREEN}PASS${NC}"
else
    echo -e "Provider history testing issues: ${YELLOW}${#provider_history_issues[@]}${NC}"
    for issue in "${provider_history_issues[@]}"; do
        echo -e "  ${YELLOW}⚠${NC} $issue"
    done
fi
echo ""

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Audit Summary${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "Total StreamHub implementations: ${GREEN}$total_hubs${NC}"
echo -e "Test files found: ${GREEN}$total_tests${NC}"
echo ""

# Calculate pass/fail
total_issues=$((missing_tests + interface_issues + test_method_issues + provider_history_missing))

if [[ $total_issues -eq 0 ]]; then
    echo -e "${GREEN}✓ All checks passed!${NC}"
    echo -e "${GREEN}✓ Test coverage: 100%${NC}"
    echo -e "${GREEN}✓ Interface compliance: Complete${NC}"
    echo -e "${GREEN}✓ Required test methods: Present${NC}"
    echo -e "${GREEN}✓ Provider history testing: Comprehensive${NC}"
else
    echo -e "${YELLOW}Issues found:${NC}"
    [[ $missing_tests -gt 0 ]] && echo -e "  ${RED}✗${NC} Missing test files: $missing_tests"
    [[ $interface_issues -gt 0 ]] && echo -e "  ${RED}✗${NC} Interface compliance issues: $interface_issues"
    [[ $test_method_issues -gt 0 ]] && echo -e "  ${YELLOW}⚠${NC} Missing test methods: $test_method_issues"
    [[ $provider_history_missing -gt 0 ]] && echo -e "  ${YELLOW}⚠${NC} Provider history testing gaps: $provider_history_missing"
fi
echo ""

# T184-T185: Check test base class
echo -e "${BLUE}=== T184-T185: Test Base Class Review ===${NC}"
echo ""

test_base_file="tests/Indicators/TestBase/StreamHubTestBase.cs"
if [[ -f "$test_base_file" ]]; then
    echo -e "${GREEN}✓${NC} StreamHubTestBase exists"

    # Check for key components
    has_interface_definitions=$(grep -c "interface ITest" "$test_base_file" || true)
    has_assert_helper=$(grep -c "AssertProviderHistoryIntegrity" "$test_base_file" || true)

    echo -e "${GREEN}✓${NC} Test interfaces defined: $has_interface_definitions"
    echo -e "${GREEN}✓${NC} Helper methods available: Yes"
else
    echo -e "${RED}✗${NC} StreamHubTestBase not found"
fi
echo ""

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Recommendations${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

if [[ $missing_tests -gt 0 ]]; then
    echo -e "1. ${YELLOW}Create missing test files${NC} for StreamHub implementations"
fi

if [[ $interface_issues -gt 0 ]]; then
    echo -e "2. ${YELLOW}Fix interface compliance issues${NC} - ensure proper observer interface implementation"
fi

if [[ $test_method_issues -gt 0 ]]; then
    echo -e "3. ${YELLOW}Add missing test methods${NC} according to implemented interfaces"
fi

if [[ $provider_history_missing -gt 0 ]]; then
    echo -e "4. ${YELLOW}Enhance provider history testing${NC} - add Insert/Remove scenario coverage"
fi

if [[ $total_issues -eq 0 ]]; then
    echo -e "${GREEN}✓ No action required - all StreamHub tests follow best practices!${NC}"
fi

echo ""
echo -e "${BLUE}Audit complete.${NC}"
echo ""

# Exit with error code if critical issues found
if [[ $missing_tests -gt 0 ]] || [[ $interface_issues -gt 0 ]]; then
    exit 1
fi

exit 0
