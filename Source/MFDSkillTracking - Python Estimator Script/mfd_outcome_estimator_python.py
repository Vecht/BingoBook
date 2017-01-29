from random import randint
import re


def RunSimulation(charDice, opponentDice):
    charWins = 0;
    classAVictory = 0;
    classBVictory = 0;
    classCVictory = 0;
    classADefeats = 0;
    classBDefeats = 0;
    classCDefeats = 0;
    for iterations in range(100000):

        charResult = 0
        for c in range(charDice):
            charResult += randint(1, 100)

        opponentResult = 0
        for o in range(opponentDice):
            opponentResult += randint(1, 100)

        if charResult >= opponentResult:
            charWins += 1

        outcome = (charResult - opponentResult)/((charDice + opponentDice)**0.80)
        if outcome > 0 and outcome < 20:
            classAVictory += 1
        elif outcome >= 20 and outcome < 50:
            classBVictory += 1
        elif outcome >= 50:
            classCVictory +=1
        elif outcome < 0 and outcome > -20:
            classADefeats += 1
        elif outcome <= -20 and outcome > -50:
            classBDefeats += 1    
        elif outcome < -50:
            classCDefeats += 1

    pWins = str(round(100*charWins/(iterations * 1.0), 2))
    pAV = str(round(100*classAVictory/(iterations * 1.0), 2))
    pBV = str(round(100*classBVictory/(iterations * 1.0), 2))
    pCV = str(round(100*classCVictory/(iterations * 1.0), 2))
    pAD = str(round(100*classADefeats/(iterations * 1.0), 2))
    pBD = str(round(100*classBDefeats/(iterations * 1.0), 2))
    pCD = str(round(100*classCDefeats/(iterations * 1.0), 2))

    msg = "Win Chance:\t\t" + pWins +\
          "%\n% Class A Victory:\t" + pAV +\
          "%\n% Class B Victory:\t" + pBV +\
          "%\n% Class C Victory:\t" + pCV +\
          "%\n% Class A Defeat:\t" + pAD +\
          "%\n% Class B Defeat:\t" + pBD +\
          "%\n% Class C Defeat:\t" + pCD + "%\n\n"

    return msg


def Main():
    prompt = "Enter X to exit, or else \"#CharDice,#OpponentDice\" to get an estimate:\n\r"
    userInput = input(prompt)
    while(userInput != "X"):
        try:
            data = re.sub(r'\s', '', userInput).split(',')
            charDice = int(data[0])
            opponentDice = int(data[1])
            print(RunSimulation(charDice, opponentDice))
        except ex:
            print(ex)
            print("Invalid input!\n\r")
        userInput = input(prompt)

Main()
    
