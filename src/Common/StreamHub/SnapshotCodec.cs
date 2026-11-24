namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Reads and writes a hub's internal state as a version byte followed by raw values.
/// </summary>
/// <remarks>
/// Doubles are written as their exact bit patterns, so a restored state
/// continues bit-for-bit, including NaN.
/// </remarks>
internal static class SnapshotCodec
{
    /// <summary>
    /// Encodes state with a leading version.
    /// </summary>
    /// <param name="version">Hub-specific state version.</param>
    /// <param name="write">Writes the state values.</param>
    /// <returns>The encoded state.</returns>
    internal static byte[] Write(byte version, Action<BinaryWriter> write)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(version);
            write(writer);
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Decodes state written by <see cref="Write"/>.
    /// </summary>
    /// <param name="state">Encoded state.</param>
    /// <param name="version">The only version the caller can read.</param>
    /// <param name="read">Reads the state values.</param>
    /// <exception cref="InvalidDataException">
    /// The state has another version, or is shorter or longer than the reader expects.
    /// </exception>
    internal static void Read(ReadOnlySpan<byte> state, byte version, Action<BinaryReader> read)
    {
        using MemoryStream stream = new(state.ToArray());
        using BinaryReader reader = new(stream);

        if (stream.Length == 0)
        {
            throw new InvalidDataException("State is empty.");
        }

        byte found = reader.ReadByte();

        if (found != version)
        {
            throw new InvalidDataException(
                $"State version {found} is not supported; expected {version}.");
        }

        try
        {
            read(reader);
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidDataException("State is shorter than expected.", ex);
        }

        if (stream.Position != stream.Length)
        {
            throw new InvalidDataException("State is longer than expected.");
        }
    }

    /// <summary>
    /// Writes a rollback ring as an entry count, then each entry's timestamp ticks and state, oldest first.
    /// </summary>
    /// <typeparam name="TState">Rollback state type.</typeparam>
    /// <param name="writer">State writer.</param>
    /// <param name="ring">Ring to write.</param>
    /// <param name="writeState">Writes one entry's state.</param>
    internal static void WriteRing<TState>(
        BinaryWriter writer, RollbackRing<TState> ring, Action<BinaryWriter, TState> writeState)
    {
        (DateTime Timestamp, TState State)[] entries = [.. ring.Entries()];
        writer.Write(entries.Length);

        foreach ((DateTime timestamp, TState state) in entries)
        {
            writer.Write(timestamp.Ticks);
            writeState(writer, state);
        }
    }

    /// <summary>
    /// Reads rollback entries written by <see cref="WriteRing"/>.
    /// </summary>
    /// <typeparam name="TState">Rollback state type.</typeparam>
    /// <param name="reader">State reader.</param>
    /// <param name="capacity">Capacity of the ring the entries are restored into.</param>
    /// <param name="readState">Reads one entry's state.</param>
    /// <returns>The entries, oldest first, to load with <see cref="LoadRing"/> once the whole state decodes.</returns>
    /// <exception cref="InvalidDataException">The entry count or a timestamp is out of range.</exception>
    internal static List<(DateTime Timestamp, TState State)> ReadRing<TState>(
        BinaryReader reader, int capacity, Func<BinaryReader, TState> readState)
    {
        int count = reader.ReadInt32();

        if (count < 0 || count > capacity)
        {
            throw new InvalidDataException($"Rollback entry count {count} is out of range.");
        }

        List<(DateTime Timestamp, TState State)> entries = [with(count)];

        for (int i = 0; i < count; i++)
        {
            long ticks = reader.ReadInt64();

            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
            {
                throw new InvalidDataException("Rollback entry timestamp is out of range.");
            }

            entries.Add((new DateTime(ticks, DateTimeKind.Unspecified), readState(reader)));
        }

        return entries;
    }

    /// <summary>
    /// Replaces a ring's entries with decoded ones.
    /// </summary>
    /// <typeparam name="TState">Rollback state type.</typeparam>
    /// <param name="ring">Ring to replace.</param>
    /// <param name="entries">Entries from <see cref="ReadRing"/>, oldest first.</param>
    internal static void LoadRing<TState>(
        RollbackRing<TState> ring, List<(DateTime Timestamp, TState State)> entries)
    {
        ring.Clear();

        foreach ((DateTime timestamp, TState state) in entries)
        {
            ring.Add(timestamp, state);
        }
    }
}
