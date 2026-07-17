import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../app/authStore';
import type { ChatMessageDto } from '../types/chat';

const hubUrl = `${import.meta.env.VITE_API_URL}/hubs/notifications`;

// Single shared SignalR connection for the whole app (notifications + chat) — one socket instead of
// two, mounted once in AppLayout. Notification pushes are a lightweight "you have something new"
// ping (client just invalidates and refetches over REST); chat pushes carry the actual message
// since a thread needs to update without a round-trip. The 60s poll already on notification queries
// (NotificationBell.tsx) stays in place as a fallback in case the socket is down.
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

    connection.on('newChatMessage', (message: ChatMessageDto) => {
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversation', message.senderId] });
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversations'] });
    });

    connection.start().catch(() => {
      // Silent — the 60s poll in NotificationBell covers delivery if the realtime channel never connects.
    });

    return () => {
      connection.stop();
    };
  }, [isAuthenticated, queryClient]);
}
