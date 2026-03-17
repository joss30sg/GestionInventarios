import { useState, useEffect } from 'react';
import './NotificationCenter.css';

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

interface NotificationCenterProps {
  notifications: Notification[];
  onDismiss?: (id: string) => void;
  maxVisible?: number;
}

export function NotificationCenter({ notifications, onDismiss, maxVisible = 3 }: NotificationCenterProps) {
  const [visibleNotifications, setVisibleNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    // Mostrar solo las últimas notificaciones (máximo maxVisible)
    setVisibleNotifications(notifications.slice(-maxVisible));
  }, [notifications, maxVisible]);

  const handleDismiss = (id: string) => {
    setVisibleNotifications(prev => prev.filter(n => n.id !== id));
    onDismiss?.(id);
  };

  const getSeverityIcon = (severity: string) => {
    return severity === 'Critical' ? '🚨' : '⚠️';
  };

  const getSeverityClass = (severity: string) => {
    return severity === 'Critical' ? 'critical' : 'warning';
  };

  return (
    <div className="notification-center">
      {visibleNotifications.map(notification => (
        <div
          key={notification.id}
          className={`notification-card ${getSeverityClass(notification.severity)}`}
        >
          <div className="notification-header">
            <span className="severity-icon">
              {getSeverityIcon(notification.severity)}
            </span>
            <span className="severity-label">{notification.severity}</span>
            <button
              className="close-btn"
              onClick={() => handleDismiss(notification.id)}
              aria-label="Cerrar notificación"
            >
              ✕
            </button>
          </div>

          <div className="notification-content">
            <h4 className="product-name">{notification.productName}</h4>
            
            <div className="stock-info">
              <span className="stock-label">Stock Actual:</span>
              <span className="stock-value">{notification.currentQuantity} unidades</span>
            </div>

            <div className="stock-bar">
              <div
                className="stock-fill"
                style={{
                  width: `${Math.max(
                    (notification.currentQuantity / notification.thresholdQuantity) * 100,
                    0
                  )}%`,
                }}
              />
            </div>

            <div className="notification-details">
              <span className="category">📦 {notification.category}</span>
              <span className="threshold">Umbral: {notification.thresholdQuantity} unidades</span>
            </div>

            <div className="notification-time">
              {new Date(notification.alertTime).toLocaleTimeString('es-MX')}
            </div>
          </div>
        </div>
      ))}

      {notifications.length > maxVisible && (
        <div className="notification-overflow">
          +{notifications.length - maxVisible} más
        </div>
      )}
    </div>
  );
}
