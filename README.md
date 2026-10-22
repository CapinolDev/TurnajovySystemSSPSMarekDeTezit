# Tournament Manager

The console application can save and load tournaments in the `.sspst` file format.
The default file name is `tournament.sspst` in the current working directory.

```sh
dotnet run -- save
dotnet run -- load
```

Pass a file path after `save` or `load` to use a different file:

```sh
dotnet run -- save backups/autumn-tournament.sspst
dotnet run -- load backups/autumn-tournament.sspst
```

The `.sspst` file is a versioned, semicolon-separated text format. It contains
the tournament name and date, along with each player's name, points, games
played, and status. Text fields are quoted and embedded quotes are doubled, so
semicolons and line breaks in names are preserved. Unsupported or invalid files
are reported rather than silently loaded.
