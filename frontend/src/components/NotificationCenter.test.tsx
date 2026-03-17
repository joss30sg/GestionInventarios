import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { NotificationCenter } from './NotificationCenter';

const createNotification = (overrides = {}) => ({
  id: '1',
  productId: 1,
  productName: 'Producto Test',
  currentQuantity: 2,
  thresholdQuantity: 5,
  category: 'General',
  alertTime: new Date().toISOString(),
  severity: 'Warning' as const,
  ...overrides,
});

describe('NotificationCenter', () => {
  it('renders notifications', () => {
    const notifications = [createNotification()];
    render(<NotificationCenter notifications={notifications} />);
    expect(screen.getByText('Producto Test')).toBeInTheDocument();
  });

  it('shows severity icon for Warning', () => {
    const notifications = [createNotification({ severity: 'Warning' })];
    render(<NotificationCenter notifications={notifications} />);
    expect(screen.getByText('⚠️')).toBeInTheDocument();
  });

  it('shows severity icon for Critical', () => {
    const notifications = [createNotification({ severity: 'Critical' })];
    render(<NotificationCenter notifications={notifications} />);
    expect(screen.getByText('🚨')).toBeInTheDocument();
  });

  it('shows current quantity', () => {
    const notifications = [createNotification({ currentQuantity: 3 })];
    render(<NotificationCenter notifications={notifications} />);
    expect(screen.getByText('3 unidades')).toBeInTheDocument();
  });

  it('calls onDismiss when close button is clicked', async () => {
    const user = userEvent.setup();
    const onDismiss = vi.fn();
    const notifications = [createNotification({ id: 'abc' })];

    render(<NotificationCenter notifications={notifications} onDismiss={onDismiss} />);
    await user.click(screen.getByLabelText('Cerrar notificación'));

    expect(onDismiss).toHaveBeenCalledWith('abc');
  });

  it('limits visible notifications to maxVisible', () => {
    const notifications = [
      createNotification({ id: '1', productName: 'Producto 1' }),
      createNotification({ id: '2', productName: 'Producto 2' }),
      createNotification({ id: '3', productName: 'Producto 3' }),
      createNotification({ id: '4', productName: 'Producto 4' }),
    ];

    render(<NotificationCenter notifications={notifications} maxVisible={2} />);
    expect(screen.getByText('+2 más')).toBeInTheDocument();
  });

  it('renders empty when no notifications', () => {
    const { container } = render(<NotificationCenter notifications={[]} />);
    const center = container.querySelector('.notification-center');
    expect(center?.children.length).toBe(0);
  });
});
