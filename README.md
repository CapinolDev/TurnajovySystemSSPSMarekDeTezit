# Tournament Manager

The CLI stores one tournament per `.sspst` file. Commands load and save the
tournament automatically; the default file is `tournament.sspst` in the
current directory. Use `--file <path>` to manage a different tournament.

## Manage a tournament

```sh
dotnet run -- create "Autumn Cup" 2026-10-15 --file autumn.sspst
dotnet run -- player add "Alice" --file autumn.sspst
dotnet run -- player add "Bob" --file autumn.sspst
dotnet run -- players --file autumn.sspst
dotnet run -- info --file autumn.sspst
```

Create refuses to overwrite an existing file. Start a separate tournament by
choosing another file:

```sh
dotnet run -- create "Spring Cup" 2027-04-20 --file spring.sspst
dotnet run -- player add "Alice" --file spring.sspst
```

Players can be added or removed only before the first round. A player with
played games cannot be removed. Omit `--file autumn.sspst` in these examples to
use the default `tournament.sspst` file.

## Play rounds and record results

Rounds are generated in round-robin order, with no repeated pairings and fair
byes when the player count is odd. Close each round before creating the next;
every match must have a result before a round can be closed.

```sh
dotnet run -- round create
dotnet run -- round list
dotnet run -- result 1 1 a       # player A wins match 1 in round 1
dotnet run -- result 1 2 draw    # draw
dotnet run -- result 1 3 b       # player B wins
dotnet run -- round close 1
dotnet run -- standings
```

Results can be corrected while a round is open. A win is worth 1 point, a draw
is worth 0.5 points, and a bye is worth 1 point. Standings are ordered by points
and then player name.

## Commands

```text
create <name> <date>       Create a tournament (date such as 2026-10-15)
info                        Show tournament details
players                     List players
player add <name>           Add a player
player remove <name>        Remove a player
round create                Create the next round-robin round
round list                  List rounds and match results
result <round> <match> <a|b|draw>
round close <number>        Close a round
standings                   Show points and games played
save [file]                 Save the default or specified tournament
load [file]                 Load and show the default or specified tournament
help
```

For commands other than `save` and `load`, pass an alternate path with
`--file <path>`. The positional path form remains available for `save` and
`load`. Invalid input or tournament files are reported without being replaced.

The versioned `.sspst` format stores players, rounds, results, and standings.
Existing version 1 files remain loadable; they are upgraded to the current
format the next time they are saved.
