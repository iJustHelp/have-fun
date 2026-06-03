# Formula Scrambler Game
- create new game as others
- player can see tiles from formula and needs to collect correct formula
ex. 
scramble: + 3 / 1 ( 2 ) 2 = 
correct formula: (1+3)/2=2


Missing decisions:

What counts as a tile: single characters only, or grouped tokens like 12, +, (, )?
each char is counted like in other games

What counts as correct: exact text order only, or mathematically equivalent formulas too?
mathematically equivalent formulas

Formula source: new asset folder/config path, or reuse existing file service pattern?
new 

Host flow: same as Word Scrambler with file selection, timeout, start/stop, results?
yes

Player flow: same PlayerGameBoard tile selection behavior?
yes

Scoring: per correct token position, full formula only, or something else?
full correct formula but score is how many chars

Routes/names: likely /host-formula-scrambler and /player-formula-scrambler.
yes
