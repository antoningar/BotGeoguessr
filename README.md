# Description
Bot Geoguessr is a c# disord bot.
The goal is to allow any members to create a coop Geoguessr game.
Only one premium account is needed, the bot's account.
The bot is in charge of managing the party as a real player (host, launch and skip steps).

# Commands
Command | Description | Inputs | Prerequisites
------------ | ------------- | ------------- | -------------
settings | update game settings | maps, duration | not being in game
maps | get the list of maps |  |
creategame | create a new game and return the join link, just return the link if a game already exist | | 
startgame | start the game | | game must be created
abort | abort the game | | game must be started
