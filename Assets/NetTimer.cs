﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618 // Type or member is obsolete
public class NetTimer : NetworkBehaviour
{

	public WallTimer wallTimer;
	public float TimeAllotted = 3 * 60;

	[Command]
	public void CmdStartTheTimers()
	{

		RpcStartTimers(TimeAllotted);
		PersonalStartTimers(TimeAllotted);
	}

	[Command]
	public void CmdStopTimer()
	{
		RpcStopTimer();
        NetworkServer.SendByChannelToAll(MPMsgTypes.GameSucceed, new SimpleStringMessage() { payload = "You win." }, 0);
        // :D

        //RoomManager.instance.CMMP.nm.net.SendToServer(MPMsgTypes.GameSucceed, new SimpleStringMessage() { payload = "You win." });
        NetworkMessage nm = new NetworkMessage();
        NetworkWriter nw = new NetworkWriter();
        //nw.StartMessage(MPMsgTypes.GameSucceed);
        new SimpleStringMessage() { payload = "You win." }.Serialize(nw);
        //nw.FinishMessage();
        nm.reader = new NetworkReader(nw);
        RoomManager.instance.CMMP.ShowWinScreen(nm);
        wallTimer.StopTimer();
	}

	[ClientRpc]
	public void RpcStopTimer()
	{
		wallTimer.StopTimer();
	}

	public void PersonalStartTimers(float delta)
	{
		wallTimer.StartClock(delta);
	}

	[ClientRpc]
	public void RpcStartTimers(float delta)
	{
		wallTimer.StartClock(delta);
	}
}
#pragma warning restore CS0618 // Type or member is obsolete