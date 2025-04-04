# BlmCopiumPlugin

 Simple plugin to revive the enochain timer for BLM.
 
 ![image](https://github.com/JoeXL/BlmCopiumPlugin/blob/master/BlmCopiumConfig.png)
 ![image](https://github.com/JoeXL/BlmCopiumPlugin/blob/master/BlmCopiumTimer.png)

 ## Features

 * Customizable enochain duration
 * Toggle if casts are interrupted/blocked (casts are interrupted via jumping)

The actions which start the timer are:
* Fire
* Fire III
* Blizzard
* Blizzard III
* Paradox
* Flare
* High Fire II
* High Blizzard II
* Freeze
* Despair
* Manafont
* Transpose
* Umbral Soul (also pauses the timer)

The actions blocked when the timer is zero are:
* Fire IV
* Blizzard IV
* Flare Star
* Despair
* Paradox
* Flare
* Freeze

## Installation
Dalamud Settings > Experimental > Custom Plugin Repositories
Add "https://raw.githubusercontent.com/JoeXL/BlmCopiumPlugin/refs/heads/master/repo.json" as a URL (without quotes)
It should now show up in the dalamud plugin list for you to install

## Limitations
Given this plugin only manipulates the action of casting actions, it will not actually remove the enochain state when the timer runs out. This means polyglot stacks stay and continue to accumulate, and also if in fire, fire spells will still have an increased mana cost.