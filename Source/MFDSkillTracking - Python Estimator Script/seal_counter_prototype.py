from random import randint
import math


def im_feeling_suicidal():
    difficulty = int(input("input difficulty [1-100]\n> "))
    skill = int(input("input skill [1-100]\n> "))
    counter = 0
    effort = difficulty/100 * 50 * randint(1,42)
    hitPoints = 30
    day = 0

    while (counter < effort):

        print("Day: " + str(day) + ". You have " + str(hitPoints) + " hit points.")

        #Simulate die rolls
        skillDice = 0
        for a in range(skill):
            skillDice += randint(1,101)
        difficultyDice = 0
        for b in range(difficulty):
            difficultyDice += randint(1,101)
        outcome = (skillDice - difficultyDice)/((skill + difficulty)**0.65)

        #Decide if terrible things happen, or if progress is made
        if (outcome < -50):
            print("\n\tBOOM! SQUISH! MEATPASTE!\n")
            hitPoints = 0;
        elif (outcome < -20):
            print("\n\tYou feel goo leaking out of your ears. -20 hp\n")
            hitPoints -= 20;
        elif (outcome < 0 and hitPoints > 5):
            print("\n\tClose encounter with a Lupchanz. -5 hp\n")
            hitPoints -= 5;
        elif (outcome < 0):
            print("\n\tYou have been infected by a Lupchanz.\n")
            hitPoints -= 5;
        else:
            counter += outcome

        #Did you win or die?
        if hitPoints <= 0:
            print("You are dead.")
            break
        if counter >= effort:
            print("You learned the seal without dying!")
            break

        #Show fuzzed progress to the player
        progress = counter/(1.0 * effort)
        
        fuzz_factor = math.floor(-difficulty*math.log(0.000001+progress))
        if (fuzz_factor < 1):
            fuzz_factor = 1
            
        gain_factor = math.floor(-2*skill*math.log(1-progress))
        if (gain_factor < 1):
            gain_factor = 1
            
        fuzzed_progress = round((fuzz_factor * randint(1,101) + gain_factor * progress * 100)/(fuzz_factor + gain_factor), 1)

        ratio = gain_factor / fuzz_factor
        
        print("\tYou think you are " + str(fuzzed_progress) + "% done.")
        if (ratio < 0.10):
            print("\t...And you are extremely confident of this assessment.")
        elif (ratio < 0.5):
            print("\t...And you have no idea how you have survived thus far.")
        elif (ratio < 1.0):
            print("\t...And you're pretty sure you have no clue what you're doing.")
        elif (ratio < 3.0):
            print("\t...And you think you're starting to grasp the basic principles.")
        elif (ratio < 5.0):
            print("\t...And you feel like you're making hard-won progress.")
        else:
            print("\t...And you are extremely confident of this assessment.")          

        #print(fuzz_factor)
        #print(gain_factor)
        #print(ratio)
        #print(progress)
            
        #Time marches on
        day += 1
        if (hitPoints < 30):
            hitPoints += 1

        input()

    print("The target effort was: " + str(effort))
    print("\t~= " + str(round(effort/5.0,0)) + " A-Class victories")
    print("\t~= " + str(round(effort/20.0,0)) + " B-Class victories")
    print("\t~= " + str(round(effort/50.0,0)) + " C-Class victories")
    print("\nHint: Type \"im_feeling_suicidal()\" to play again")

im_feeling_suicidal()
