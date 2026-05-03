# Have-Fun Project

## My Notes

Create a client-server app without using cloud deployment or other infrastructure, so it can be completely free.

- The server should be hosted on any Windows or Mac computer.
- The client should run in a browser.

This is possible only if the server and clients are on the same LAN. Therefore, the app should provide a shared URL that users can open in their browsers.

Create a set of games with friends.

### Word Scramble Game
2 roles
- Game Master
- Players

Game Master selects predefined text from a JSON file on the Master dashboard and starts the game.
Player sees random words from the text and clicks on each word to collect the text in order. He can see collected text on screen.
Player finishes with the last word and sends the result to the dashboard.
Dashboard shows the correct text and a sortable grid with player's text, spent time, and correctness.
Define who won.
Game Master selects another sentence.

### Assumption
- No database in backend, all data is in memory.
- Player and master provide only a name.


