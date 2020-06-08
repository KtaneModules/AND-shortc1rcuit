/*MESSAGE TO ANY FUTURE CODERS:
 PLEASE COMMENT YOUR WORK
 I can't stress how important this is especially with bomb types such as boss modules.
 If you don't it makes it realy hard for somone like me to find out how a module is working so I can learn how to make my own.
 Please comment your work.
 Short_c1rcuit*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using KModkit;

public class ANDscript : MonoBehaviour
{

    //The actual module iself
    public KMBombModule module;
    //The buttons at the bottom of the module
    public KMSelectable[] buttons;
    //Used to get edgework + other info
    public KMBombInfo bombInfo;
    //Used to play audio clips
    public KMAudio audio;

    //All of the displays used in the module
    public TextMesh gateText;
    public TextMesh stageText;
    public TextMesh topDisplay;
    public TextMesh bottomDisplay;

    //An empty that holds the animations in the module
    public GameObject animHolder;

    //Holds all the text that will be displayed
    private string[,] displayed;

    //Holds the answer for each stage
    private int[] solution;

    //Stores all modules that are bosses/other modules that need to be ignored
    public static string[] ignoredModules = null;
    //Tables for each logic gate
    private int[,] gates;
    //The symbols for each logic gate
    private string[] gatesText;
    //The amount of stages in the module
    private int count = 0;
    //Used so that Update() isn't checking every frame
    private int ticker = 0;
    //Makes sure people aren't inputting when they aren't meant to.
    private bool inputting = false;
    //The stage number you are on
    private int curStage = 0;
    //The stage number for the stage you are inputting
    private int curInput = 0;

    //Used to start the animation 
    Animator anim;

    //logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private void GenerateStages()
    {
        //This loop makes every stage of the bomb.
        for (int i = 0; i < count; i++)
        {
            Debug.LogFormat("[A>N<D #{0}] Stage {1}:", moduleId, i + 1);
            //Picks a random logic gate for the stage
            int gate = UnityEngine.Random.Range(0, (gates.Length / 4) + 1);
            Debug.LogFormat("[A>N<D #{0}] The logic gate is {1}", moduleId, gatesText[gate]);
            //The 2 binary inputs and the 1 output
            string num1 = "";
            string num2 = "";
            int num3 = 0;
            //for each digit of the binary numbers
            for (int j = 0; j < 5; j++)
            {
                //Generates a random binary digit and adds it to end of the current binary number
                int c1 = UnityEngine.Random.Range(0, 2);
                int c2 = UnityEngine.Random.Range(0, 2);
                num1 += c1.ToString();
                num2 += c2.ToString();
                //If the gate is a NOT gate
                if (gate == 8)
                {
                    //Applies the not gate to the top binary number
                    num3 += (int)Mathf.Pow(2, 4 - j) * ((c1 + 1) % 2);
                }
                else
                {
                    //Applies the logic gate and adds it to the total
                    num3 += (int)Mathf.Pow(2, 4 - j) * gates[gate, (c1 * 2) + c2];
                }
            }

            //If the gate is a NOT gate
            if (gate == 8)
            {
                Debug.LogFormat("[A>N<D #{0}] ¬ {1} = {2}", moduleId, num1, num3);
            }
            else
            {
                Debug.LogFormat("[A>N<D #{0}] {1} {2} {3} = {4}", moduleId, num1, gatesText[gate], num2, num3);
            }

            //If the stage number is even (stage numbers count from 1)
            if ((i + 1) % 2 == 0)
            {
                //Modulo 10 the result
                num3 %= 10;
                Debug.LogFormat("[A>N<D #{0}] Using mod 10, the answer for this stage is {1}", moduleId, num3);
            }
            else
            {
                //Else find the digital root of the number
                while (num3 > 9)
                {
                    num3 = Mathf.FloorToInt(num3 / 10) + (num3 % 10);
                }
                Debug.LogFormat("[A>N<D #{0}] Using the digital root, the answer for this stage is {1}", moduleId, num3);
            }

            //Stores the two starting numbers, the logic gate and the resulting number
            displayed[i, 0] = num1;
            displayed[i, 1] = num2;
            solution[i] = num3;
            displayed[i, 2] = gatesText[gate];
        }
    }

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate { OnPress(pressedButton); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        audio.PlaySoundAtTransform("bomb_starts_sound", module.transform);
    }

    void Start()
    {
        //The modules we ignore when getting the number of stages
        ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("A>N<D", new string[]{
            "14",
            "A>N<D",
            "Bamboozling Time Keeper",
            "Brainf---",
            "Forget Enigma",
            "Forget Everything",
            "Forget It Not",
            "Forget Me Not",
            "Forget Me Later",
            "Forget Perspective",
            "Forget The Colors",
            "Forget Them All",
            "Forget This",
            "Forget Us Not",
            "Iconic",
            "Kugelblitz",
            "Multitask",
            "OmegaForget",
            "Organization",
            "Purgatory",
            "RPS Judging",
            "Simon Forgets",
            "Simon's Stages",
            "Souvenir",
            "Tallordered Keys",
            "The Time Keeper",
            "The Troll",
            "The Twin",
            "The Very Annoying Button",
            "Timing Is Everything",
            "Turn The Key",
            "Ultimate Custom Night",
            "Übermodule"
        });

        //Gets the number of stages for the module
        count = bombInfo.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).ToList().Count;

        //Truth table for all the logic gates except NOT
        gates = new int[8, 4]
        {
            {0,0,0,1},
            {0,1,1,1},
            {0,1,1,0},
            {1,1,1,0},
            {1,0,0,0},
            {1,0,0,1},
            {1,1,0,1},
            {1,0,1,1}
        };

        //All the symbols for the logic gate.
        gatesText = new string[9]
        {
            "∧","∨","⊻","|","↓","↔","→","←","¬"
        };

        //Gets the animator component for the button animations.
        anim = animHolder.GetComponent<Animator>();

        //If there are no modules that aren't bosses/cause problems if they aren't ignored.
        if (count == 0)
        {
            //Solves the module.
            Debug.LogFormat("[A>N<D #{0}] Some error occured where the solveable module count is 0. Automatically solving.", moduleId);
            module.HandlePass();
            moduleSolved = true;
        }
        else
        {
            //Creates an array to hold all the displayed info
            displayed = new string[count, 3];
            //Creates an array to hold all the answers
            solution = new int[count];
            //Fills in the arrays
            GenerateStages();
            //Displays the 1st stage
            gateText.text = displayed[0, 2];
            topDisplay.text = displayed[0, 0];
            bottomDisplay.text = displayed[0, 1];
            stageText.text = "1";
        }
    }

    void Update()
    {
        //A ticker is used so it doesn't lag out the game by checking every frame
        ticker++;
        if (ticker == 5)
        {
            //Resets the tickets
            ticker = 0;
            if (!inputting)
            {
                //Gets the number of solved modules (that aren't in the ignored list)
                int check = bombInfo.GetSolvedModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
                //If a new module has been solved
                if (check != curStage)
                {
                    //Updates to the new solve count
                    curStage = check;
                }
                //If we've shown all the stages
                if (check == count)
                {
                    //Allow inputting
                    inputting = true;
                    //Play the key animation
                    StartCoroutine(FlapAnim());
                    //Blank all the displays
                    stageText.text = "--";
                    gateText.text = "";
                    topDisplay.text = "";
                    bottomDisplay.text = "";
                }
                else
                {
                    //Shows the new stage
                    stageText.text = (curStage + 1).ToString();
                    gateText.text = displayed[curStage, 2];
                    topDisplay.text = displayed[curStage, 0];
                    bottomDisplay.text = displayed[curStage, 1];
                }
            }
        }
    }

    private void OnPress(KMSelectable pressedButton)
    {
        //Doesn't do anything if the module is solved
        if (moduleSolved)
        {
            return;
        }
        //Gets the index of the button in the array
        int button = Array.IndexOf(buttons, pressedButton);
        //Makes the bomb move and plays a noise
        pressedButton.AddInteractionPunch();
        audio.PlaySoundAtTransform("button_press", module.transform);

        //If you aren't meant to input yet
        if (!inputting)
        {
            Debug.LogFormat("[A>N<D #{0}] You shouldn't be inputting yet:", moduleId);
            module.HandleStrike();
            return;
        }
        //If the correct button was pressed
        if (solution[curInput] == button)
        {
            Debug.LogFormat("[A>N<D #{0}] You inputted {1}. Correct", moduleId, button);

            //Go to the next needed input
            curInput++;
            //If all items have been inputted
            if (curInput == solution.Length)
            {
                //Solve the module
                Debug.LogFormat("[A>N<D #{0}] Module solved!", moduleId);
                audio.PlaySoundAtTransform("solve-sound", module.transform);
                module.HandlePass();
                moduleSolved = true;
            }
            //Clear all displays
            stageText.text = "--";
            gateText.text = "";
            topDisplay.text = "";
            bottomDisplay.text = "";
        }
        else
        {
            Debug.LogFormat("[A>N<D #{0}] You inputted {1} when you should have inputted {2}", moduleId, button, solution[curInput]);
            //Give a strike
            module.HandleStrike();
            //Show the stage they struck on
            ShowCurrentStage();
            return;
        }
    }

    private void ShowCurrentStage()
    {
        gateText.text = displayed[curInput, 2];
        topDisplay.text = displayed[curInput, 0];
        bottomDisplay.text = displayed[curInput, 1];
        stageText.text = (curInput + 1).ToString();
    }

    IEnumerator FlapAnim()
    {
        //Starts the animation
        anim.SetTrigger("Endgame");
        //Waits for 135 frames
        for (int i = 0; i < 135; i++)
        {
            yield return null;
        }
        //Plays a sound effect
        audio.PlaySoundAtTransform("Shutter", transform);
    }
}