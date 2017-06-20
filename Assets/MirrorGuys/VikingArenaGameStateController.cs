using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class VikingArenaGameStateController : MonoBehaviour
{
    MirrorGuyTrackedObject playerLeft { get { return MirrorGuyTrackedObject.playerLeft; } }
    MirrorGuyTrackedObject playerRight { get { return MirrorGuyTrackedObject.playerRight; } }
    public MirrorGuyTrackedObject playerHead { get { return MirrorGuyTrackedObject.playerHead; } }

    public List<MirrorGuyLiveControl> players = new List<MirrorGuyLiveControl>();

    [SerializeField]
    private List<MirrorGuyLiveControl> fightingPlayers = new List<MirrorGuyLiveControl>();

    public MirrorGuyLiveControl playerPrefab;

    public LayerMask playerSelectLayer;

    public States gameState = States.PlayerSelect;

    [Header("PlayerSpawn params")]
    public Material[] playerMats;
    public MirrorGuyAnimationShare[] idleAnims;
    public MirrorGuyAnimationShare[] victoryAnims;
    public AnimationCurve idleRadiusFromPlayerCount = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };

    [Header("PlayerSelect params")]
    public float stepsForward = 2f;
    public float lookAtGuyTimeForSelection = 0.2f;
    public float maxSelectSpeed = 0.25f;
    public MirrorGuyAnimationShare getSelectedAnim;
    MirrorGuyLiveControl selectedPlayer;
    MirrorGuyLiveControl guyLookedAtLastWhile;
    float timeLookedAtLastGuy;
    float timeLookedAtNobody;
    Vector3 selectedPlayerStartPos;

    [Header("PlayerControl params")]
    public float deselectSpeed = 1;

    [Header("FightStart params")]
    public float arenaLookBeforeFight = 0.3f;
    float arenaLookTime = 0;
    public float arenaPressDurationToFight = 1f;
    float arenaPressTime = 0f;
    ArenaFeedback arenaFeedback;

    [Header("Fight params")]
    bool fightingStarted = false;
    public float fightWalkSpeed = 0.5f;
    public AnimationCurve fightRadiusFromPlayerCount = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };

    public float fightingTimeBeforeDeadGuy = 10f;
    public float stepsEachFightReset = 0.2f;

    [Header("Victory params")]

    public float victoryMinDuration = 5f;

    public float victoryWalkBackSpeed = 0.78f;

    float checkForRagdollsTime = 0;

    public enum States
    {
        PlayerSelect,
        PlayerControl,
        Fight,
    }

    private MirrorGuySoundSets soundSets;

    private void Awake()
    {
        players = FindObjectsOfType<MirrorGuyLiveControl>().ToList();
        soundSets = GetComponent<MirrorGuySoundSets>();

    }

    private IEnumerator Start()
    {
        yield return 0;

        RepositionPlayers();

    }

    private void Update()
    {
        switch (gameState)
        {
        case States.PlayerSelect:
            HandleState_PlayerSelect();
            break;
        case States.PlayerControl:
            HandleState_PlayerControl();
            break;
        case States.Fight:
            HandleState_Fight();
            break;

        }
    }

    private bool InputPressedVictoryReset()
    {
        return RightTouchpadDown();
    }

    private static void SetEveryonesState(List<MirrorGuyLiveControl> aliveFightingPlayers, MirrorGuyLiveControl.States state)
    {
        for (int i = 0; i < aliveFightingPlayers.Count; i++)
        {
            aliveFightingPlayers[i].state = state;
        }
    }

    private void HandleState_PlayerSelect()
    {
        HandleSpawnPlayerInput();

        // unragdollify from time to time, during player selection screen.
        if (checkForRagdollsTime < Time.time)
        {
            checkForRagdollsTime = Time.time + 1f;
            if (players.Any(p => p.isRagdoll))
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].ragdollRoot.UnRagdollify();
                }
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            // just to be safe, clear the override anim. set it again if needed.
            if (guyLookedAtLastWhile != players[i] && selectedPlayer != players[i])
                players[i].overrideAnimation = null;
        }

        // looking at players chooses which one comes forward for recording a fight thing
        Vector3 lookDir = playerHead.transform.forward;
        MirrorGuyLiveControl guyLookedAt = null;
        RaycastHit info;
        if (Physics.Raycast(playerHead.transform.position, lookDir, out info, 1000f, playerSelectLayer))
        {
            guyLookedAt = info.collider.transform.root.GetComponent<MirrorGuyLiveControl>();
            if (fightingPlayers.Contains(guyLookedAt))
            {
                guyLookedAt = null;
            }

            if (info.collider.GetComponent<MirrorGuyArenaCollider>() != null)
            {
                if (arenaFeedback == null)
                {
                    arenaFeedback = info.collider.GetComponent<ArenaFeedback>();
                }
                arenaLookTime += Time.deltaTime;
                if (arenaLookTime > arenaLookBeforeFight)
                {
                    if (InputPressedFightStart())
                    {
                        arenaPressTime += Time.deltaTime;
                        // feedback on arena start
                        arenaFeedback.DoFeedback();
                    }
                    else
                    {
                        arenaFeedback.DoPreFeedback();
                        arenaPressTime = 0f;
                    }
                }
                else
                {
                    arenaPressTime = 0;
                }
            }
            else
            {
                arenaLookTime = 0;
                arenaPressTime = 0;
            }
        }
        else
        {
            guyLookedAt = null;
        }

        if (arenaPressTime > arenaPressDurationToFight)
        {
            arenaPressTime = 0f;
            arenaLookTime = 0f;
            gameState = States.Fight;
            return;
        }

        if (guyLookedAt != null)
        {
            if (guyLookedAtLastWhile != guyLookedAt)
            {
                guyLookedAtLastWhile = guyLookedAt;
                guyLookedAtLastWhile.PlayAnim(getSelectedAnim);

                timeLookedAtLastGuy = 0;
            }
            else
            {
                timeLookedAtLastGuy += Time.deltaTime;
            }
            timeLookedAtNobody = 0;
        }
        else
        {
            timeLookedAtLastGuy = 0f;
            timeLookedAtNobody += Time.deltaTime;

            if (guyLookedAtLastWhile != null)
            {
                guyLookedAtLastWhile.PlayDefaultIdleAnim();
            }
            guyLookedAtLastWhile = null;
        }

        if (selectedPlayer == null)
        {
            // if looking at someone for a while, select him for the preview.
            if (timeLookedAtLastGuy > lookAtGuyTimeForSelection)
            {
                selectedPlayer = guyLookedAt;

            }
        }
        else
        {
            // we have selected a dude with our look.
            // if we click a button, we finalize selection and start recording with him.
            if (timeLookedAtLastGuy > lookAtGuyTimeForSelection || timeLookedAtNobody > lookAtGuyTimeForSelection)
            {
                if (guyLookedAt != selectedPlayer)
                {
                    selectedPlayer = null;
                    timeLookedAtLastGuy = 0f;

                }
            }

        }

        // if selected player still not null and player decides to record, he walks forward and starts recording.
        if (selectedPlayer != null)
        {
            if (InputPressedRecord())
            {
                selectedPlayerStartPos = selectedPlayer.transform.position;
                selectedPlayer.WalkTo(selectedPlayer.transform.position + selectedPlayer.transform.forward * stepsForward, maxSelectSpeed, false, () =>
                {
                    // record when the player finishes the little walk
                    selectedPlayer.state = MirrorGuyLiveControl.States.RecordFight;
                });

                guyLookedAtLastWhile = null;

                selectedPlayer.canUseViveInputsToRecord = true;

                selectedPlayer.SelectionSound();

                selectedPlayer.state = MirrorGuyLiveControl.States.LivePreview;

                this.gameState = States.PlayerControl;
            }
        }



    }

    private void HandleSpawnPlayerInput()
    {
        if (selectedPlayer != null)
        {
            if (InputPressedDeletePlayer())
            {
                RemovePlayer(selectedPlayer);
                selectedPlayer = null;
            }
        }

        if (InputPressedCreatePlayer())
        {
            SpawnRandomPlayer();
            // player spawn spazz
        }

    }

    private bool InputPressedCreatePlayer()
    {
        return playerRight.controller.GetPressDown(ControllerWrapper.ButtonMask.ApplicationMenu);
    }

    private bool InputPressedDeletePlayer()
    {
        return playerLeft.controller.GetPressDown(ControllerWrapper.ButtonMask.ApplicationMenu);
    }

    private void HandleState_PlayerControl()
    {
        // selectedPlayer is the player.
        if (InputPressedFinishRecord())
        {
            selectedPlayer.WalkTo(selectedPlayerStartPos, deselectSpeed, true);
            fightingPlayers.Add(selectedPlayer);

            selectedPlayer.state = MirrorGuyLiveControl.States.PlaybackFight;

            selectedPlayer.canUseViveInputsToRecord = false;

            selectedPlayer.SelectionSound();

            selectedPlayer = null;

            gameState = States.PlayerSelect;
        }

    }

    private void HandleState_Fight()
    {
        if (!fightingStarted)
        {
            fightingStarted = true;

            // idle state only for the first fight
            var aliveFightingPlayers = fightingPlayers.Where(mg => !mg.isRagdoll).ToList();
            SetEveryonesState(aliveFightingPlayers, MirrorGuyLiveControl.States.Idle);

            // figure out players initial positions in a circle around the arena. based on their cur positions.
            FightThesePlayers();

        }
    }

    void FightThesePlayers(int stepCount = 0)
    {
        print("Fighting those players " + stepCount);
        var aliveFightingPlayers = fightingPlayers.Where(mg => !mg.isRagdoll).ToList();
        int curPlayersAlive = aliveFightingPlayers.Count;
        var radius = fightRadiusFromPlayerCount.Evaluate(fightingPlayers.Count) - stepCount * stepsEachFightReset;

        // walk to center
        for (int i = 0; i < aliveFightingPlayers.Count; i++)
        {
            var p = aliveFightingPlayers[i];
            var dirToCenter = transform.position - p.transform.position;
            p.WalkTo(transform.position - dirToCenter.normalized * radius, fightWalkSpeed);
        }

        // wait for all to be ready in position
        StartCoroutine(pTween.WaitCondition(() =>
        {
            if (fightingPlayers.Count == 0)
            {
                return true;
            }
            return fightingPlayers.All(b => (b.isRagdoll) || (!b.isRagdoll && !b.walking));
        }, () =>
        {
            // when everyone is ready and in position, fighto!
            SetEveryonesState(fightingPlayers, MirrorGuyLiveControl.States.PlaybackFight);

            StartCoroutine(CheckForFinishFighting(stepCount));

        }));

    }

    private IEnumerator CheckForFinishFighting(int stepCount)
    {
        var playersAlive = fightingPlayers.Where(lc => !lc.isRagdoll).Count();
        while (true)
        {
            print("Check for finish fighting  " + stepCount);

            yield return new WaitForSeconds(fightingTimeBeforeDeadGuy);
            var curPlayersAlive = fightingPlayers.Where(lc => !lc.isRagdoll).Count();

            if (curPlayersAlive == 1)
            {
                var victor = fightingPlayers.Where(lc => !lc.isRagdoll).First();
                Victory(victor);
                yield break;

            }
            else if (curPlayersAlive == 0)
            {
                // LOSE. auto restart.
                Victory(null);
                yield break;
            }
            else if (playersAlive != curPlayersAlive)
            {
                playersAlive = curPlayersAlive;

                // somebody died but not the last one. let it run for another fightingTimeBeforeDeadGuy seconds.
                continue;
            }
            else
            {
                // nobody died.
                // START FIGHTING AGAIN!!!
                FightThesePlayers(stepCount + 1);
                yield break;
            }

            yield return 0;
        }

        yield break;
    }

    private void Victory(MirrorGuyLiveControl victor)
    {
        print("VICTOREYEAH| press touchpad to reset ;)");

        // victory!!! or total annihilation.
        if (victor != null)
        {
            victor.state = MirrorGuyLiveControl.States.Idle;
            victor.overrideAnimation = null;
            victor.PlayAnim(victor.victoryDance);
            victor.VictorySound();

            MirrorGuyTrophyManager.inst.WinTrophy(victor);

        }

        StartCoroutine(pTween.Wait(victoryMinDuration, () =>
        {
            StartCoroutine(pTween.WaitCondition(() =>
            {
                return InputPressedVictoryReset() || fightingPlayers.Count == 0;
            }, () =>
            {
                RestartGame();
            }));
        }));

    }

    private void RestartGame()
    {
        for (int i = 0; i < fightingPlayers.Count; i++)
        {
            var f = fightingPlayers[i];
            f.ragdollRoot.UnRagdollify();

            // walk to starting position.....
            f.WalkTo(f.startPosition, victoryWalkBackSpeed, false, () =>
            {
                f.transform.LookAt(transform.position);
            });
        }

        StartCoroutine(pTween.WaitCondition(() =>
        {
            for (int i = 0; i < fightingPlayers.Count; i++)
            {
                var f = fightingPlayers[i];

                if (f.isRagdoll)
                {
                    // do a montecarlo for this because sometimes you need to make him a ragdoll before he can be un-made ragdoll... fucking random bug I know
                    if (Random.Range(0, 50) == 0)
                    {
                        f.ragdollRoot.Ragdollify();
                    }
                    else
                    {
                        f.ragdollRoot.UnRagdollify();
                    }
                }
            }

            if (fightingPlayers.Count == 0)
                return true;

            return fightingPlayers.All(p => !p.walking && !p.isRagdoll);
        }, () =>
        {
            SetEveryonesState(players, MirrorGuyLiveControl.States.Idle);

            // all are back at their positions
            gameState = States.PlayerSelect;

            fightingPlayers.Clear();

            fightingStarted = false;

            for (int i = 0; i < players.Count; i++)
            {
                var f = players[i];
                f.ragdollRoot.UnRagdollify();
            }

            RepositionPlayers();

            MirrorGuyTrophyManager.inst.SetFreshTroophy();

        }));
    }

    private bool InputPressedFinishRecord()
    {
        return LeftTouchpadDown();
    }

    private bool InputPressedRecord()
    {
        if (RightTouchpadDown())
        {
            return true;
        }
        return false;
    }

    private bool InputPressedFightStart()
    {
        return RightTouchpad();
    }

    private bool RightTouchpad()
    {
        return playerRight.controller.GetPress(ControllerWrapper.ButtonMask.Touchpad);
    }

    private bool RightTouchpadDown()
    {
        return playerRight.controller.GetPressDown(ControllerWrapper.ButtonMask.Touchpad);
    }

    private bool LeftTouchpadDown()
    {
        return playerLeft.controller.GetPressDown(ControllerWrapper.ButtonMask.Touchpad);
    }


    private void SpawnRandomPlayer()
    {
        var newp = Instantiate(playerPrefab);
        players.Add(newp);

        newp.idleAnimation = idleAnims[Random.Range(0, idleAnims.Length)];
        newp.SetMaterial(playerMats[Random.Range(0, playerMats.Length)]);
        newp.victoryDance = victoryAnims[Random.Range(0, victoryAnims.Length)];
        newp.soundSet = soundSets.soundSets[Random.Range(0, soundSets.soundSets.Count)];

        RepositionPlayers();

    }

    private void RepositionPlayers()
    {
        var toVivePlayer = MirrorGuyPlayerOffset.instance.transform.position - transform.position;

        for (int i = 0; i < (players.Count); i++)
        {
            var radius = idleRadiusFromPlayerCount.Evaluate(players.Count);
            // consider the VR player another one of the dudes, and the first(last) position around the circle.
            var angle = (i + 1) / (float)((players.Count + 1)) * 360f;
            var pos = transform.position + Quaternion.Euler(0, angle, 0) * toVivePlayer.normalized * radius;

            players[i].transform.position = pos;
            players[i].transform.LookAt(transform.position);

            players[i].Initialize();
        }
    }

    public void RemovePlayer(MirrorGuyLiveControl player)
    {
        players.Remove(player);
        Destroy(player.gameObject);

        RepositionPlayers();
    }


}