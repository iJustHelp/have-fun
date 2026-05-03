# Have-Fun Project

## My Notes

Create a client-server app without using cloud deployment or other infrastructure, so it can be completely free.

- The server should be hosted on any Windows or Mac computer.
- The client should run in a browser.

This is possible only if the server and clients are on the same LAN. Therefore, the app should provide a shared URL that users can open in their browsers.

Create set of games with friends.

### Word Scramble Game
2 roles
- Game Master
- Players

Game Master selects predefined in json file text on Master dashboard and starts game.
Player sees random words from text and clicks on each word collect text in order. He can see collected test in screen.
Player finish with last word and send result on dashboard.
Dashboard shows correct text, and sortable grid with  player's text, spent time and correctnesss.
Define who won.
Game Master selects other sentence.

### Assumption
- No database in back, all are in memory.
- player and master provide only name to auth

# Codex Feedback

I like it. It’s a nicely scoped “local party game server” idea: free, LAN-only, browser clients, no accounts, no database. That is a good shape because it avoids the hardest infrastructure problems and keeps the first version buildable.

The strongest part is the Game Master / Players split. It gives the app a clear flow: host starts a round, players compete, dashboard ranks results, then repeat.

A few things I’d clarify before building:

“Word Scramble” sounds like players should reconstruct the original sentence from shuffled words. That’s clear and fun.
The dashboard should probably rank by correctness first, then time.
“Correctness” needs a simple rule: exact word order match, maybe percentage of words in the correct position.
The shared URL is important. The server should show something like http://192.168.x.x:port so players can join from phones/laptops.
Since there’s no database, restarting the server should reset all games and players. That’s totally fine for v1.
My instinct for v1: build only this one game first, make the LAN join flow smooth, and make the Game Master dashboard feel good. Once that works, adding more games will be much easier.


