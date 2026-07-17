import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../app/authStore';
import type { BattleDto } from '../types/battle';

const hubUrl = `${import.meta.env.VITE_API_URL}/hubs/battle`;

// Scoped to a single battle room — connects, joins that battle's SignalR group, and writes every
// "battleUpdated" push straight into the query cache (the server always sends the full current
// state, so this is a plain setQueryData, no merging). Torn down when the room page unmounts.
export function useBattleHub(battleId: number | null) {
  const queryClient = useQueryClient();
  const isAuthenticated = useAuthStore((s) => s.accessToken !== null);

  useEffect(() => {
    if (!isAuthenticated || battleId === null) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => useAuthStore.getState().accessToken ?? '' })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('battleUpdated', (battle: BattleDto) => {
      queryClient.setQueryData(['battle', battle.id], battle);
    });

    connection.onreconnected(() => {
      connection.invoke('JoinBattleGroup', battleId).catch(() => {});
    });

    connection
      .start()
      .then(() => connection.invoke('JoinBattleGroup', battleId))
      .catch(() => {
        // Silent — the room page still works via its own polling refetch if the socket never connects.
      });

    return () => {
      connection.stop();
    };
  }, [isAuthenticated, battleId, queryClient]);
}
