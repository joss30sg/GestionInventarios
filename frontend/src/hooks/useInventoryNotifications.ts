import { useState, useEffect, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

interface Notification {
  id: string;
  productId: number;
  productName: string;
  currentQuantity: number;
  thresholdQuantity: number;
  category: string;
  alertTime: string;
  severity: 'Warning' | 'Critical';
}

interface UseInventoryNotificationsReturn {
  notifications: Notification[];
  isConnected: boolean;
  connectionError: string | null;
  dismissNotification: (id: string) => void;
  clearAllNotifications: () => void;
}

let connection: signalR.HubConnection | null = null;

export function useInventoryNotifications(): UseInventoryNotificationsReturn {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const [connectionError, setConnectionError] = useState<string | null>(null);

  useEffect(() => {
    const initializeConnection = async () => {
      try {
        if (!connection) {
          // Crear conexión a SignalR
          connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5000/hubs/inventory-notifications', {
              withCredentials: true,
              accessTokenFactory: () => {
                // Obtener el token del localStorage
                const token = localStorage.getItem('auth_token');
                return token || '';
              },
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

          // Escuchar el evento de alertas de stock bajo
          connection.on('LowStockAlert', (alert: any) => {
            console.log('[NOTIFICATION] Alerta recibida:', alert);
            
            const notification: Notification = {
              id: `${Date.now()}-${Math.random()}`,
              productId: alert.productId,
              productName: alert.productName,
              currentQuantity: alert.currentQuantity,
              thresholdQuantity: alert.thresholdQuantity,
              category: alert.category,
              alertTime: alert.alertTime || new Date().toISOString(),
              severity: alert.severity || (alert.currentQuantity === 0 ? 'Critical' : 'Warning'),
            };

            setNotifications(prev => [...prev, notification]);
          });

          // Confirmar registro de notificaciones
          connection.on('RegistrationSuccess', (data: any) => {
            console.log('[SIGNALR] Registro exitoso:', data);
          });

          // Manejar desconexión
          connection.onreconnecting(error => {
            console.log('[SIGNALR] Intentando reconectar...', error?.message);
            setIsConnected(false);
          });

          connection.onreconnected(connectionId => {
            console.log('[SIGNALR] Reconectado:', connectionId);
            setIsConnected(true);
            setConnectionError(null);
          });
        }

        // Iniciar conexión
        if (!isConnected && connection?.state === signalR.HubConnectionState.Disconnected) {
          await connection.start();
          console.log('[SIGNALR] Conectado al hub de notificaciones');
          setIsConnected(true);
          setConnectionError(null);

          // Registrarse en el hub con el rol correcto del usuario autenticado
          let userRole = 'Employee';
          try {
            const storedUser = localStorage.getItem('auth_user');
            if (storedUser) {
              const parsed = JSON.parse(storedUser);
              if (parsed.role) userRole = parsed.role;
            }
          } catch { /* fallback a Employee */ }
          await connection.invoke('RegisterForNotifications', userRole);
        }
      } catch (error: any) {
        console.error('[SIGNALR] Error de conexión:', error?.message);
        setConnectionError(error?.message || 'Error al conectar con el servidor');
        setIsConnected(false);
      }
    };

    initializeConnection();

    return () => {
      // Limpiar la conexión cuando el componente se desmonta
      if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.stop().catch(err => console.error('[SIGNALR] Error al desconectar:', err));
      }
    };
  }, []);

  const dismissNotification = useCallback((id: string) => {
    setNotifications(prev => prev.filter(n => n.id !== id));
  }, []);

  const clearAllNotifications = useCallback(() => {
    setNotifications([]);
  }, []);

  return {
    notifications,
    isConnected,
    connectionError,
    dismissNotification,
    clearAllNotifications,
  };
}
