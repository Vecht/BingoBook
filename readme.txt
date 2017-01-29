Thanks for using my app! I hope it helps. Though not required by the license, I would appreciate attribution for any derivative works.

---

ABOUT:

This application facilitates tracking of character skills, including doing Bayesian inference to determine unknown skill levels. It also features a utility that allows you to estimate the chance of one character winning against another, given information about their skills.

---

INSTRUCTIONS:

* Run "Bingo Book.exe" for the application.
* Source is included in the "Source" folder (duh).
* The "data.bingobook" file contains working data for the application. The application will automatically keep your data synchronized upon exit, as long as you don't force quit and it doesn't error out for some reason. By default, it just contains some demo data. This is not intended to be accurate, and can be deleted.

---

MAIN WINDOW:

* To manually save a new file, click the disk icon at the top of the main window.
* You can likewise load a file with the folder icon.
* Click the Kunai to open a utility that computes approximate win chances vs an opponent for given skills.
* To add a character, enter the name at the bottom of the pane and click the plus button, or press enter.
* To remove an entry, right click and select "Remove".
* To open a character page, double click that character's name, or select it and press enter.
* The bar up top allows for searching character names.

---

CHARACTER WINDOW:

* On the character skill page, there are known and unknown skills. Hopefully this is self explanatory. Adding, searching, and deleting can be done in both sections in the same manner as the main window.
* Unknown skill additions require you to select an initial guess as to the user's skill level to construct an initial prior probability distribution. This is complete conjecture atm, since skill levels have recently changed.
* To add a dice roll for an unknown skill, select desired skill. Enter the information at the bottom right according to the opponent and their skill level. Then enter the result, along with possible dice modifiers, and click the dice button. It may take a second, but the distribution and stats for that skill will update accordingly. Again, all changes are saved automatically upon exit.
* Roll results are tracked individually, so if you make a mistake and need to remove one, you can do so and the computation will be rerun. No worries about corrupting your data.

---

MISC:

For bug reports, suggestions, feedback, etc., message me on Discord @Vecht#2002, or send me an email at vechtx@gmail.com.

Note: This application was written in C# with WPF. Unfortunately, that means it only works on Windows. My sincere apologies to Mac/Linux users, but I am most experienced in this environment and the only reason I was willing to take the time to write this program is that due to my experience in this domain I can pump out C#/WPF apps exceptionally quickly. Anyone is of course free to port this to another language.