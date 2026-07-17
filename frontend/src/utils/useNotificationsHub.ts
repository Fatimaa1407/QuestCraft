import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../app/authStore';

const hubUrl = `${import.meta.env.VITE_API_URL}/hubs/notifications`;

// Pushes a lightweight "you have something new" ping over SignalR instead of the full notification
// payload — the client just invalidates its notification queries and refetches over the existing
// REST API, so the realtime channel stays decoupled from the notification DTO shape. The 60s
// polling interval already on those queries (NotificationBell.tsx) stays in place as a fallback
// in case the socket is down, so this is purely a latency improvement, never a hard dependency.
export function useNotificationsHub() {
  const queryClient = useQueryClient();
  const isAuthenticated = useAuthStore((s) => s.accessToken !== null);

  useEffect(() => {
    if (!isAuthenticated) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => useAuthStore.getState().accessToken ?? '' })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('newNotification', () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    });

    connection.start().catch(() => {
      // Silent — the 60s poll in NotificationBell covers delivery if the realtime channel never connects.
    });

    return () => {
      connection.stop();
    };
  }, [isAuthenticated, queryClient]);
}
