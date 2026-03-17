import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { InventoryCard } from './InventoryCard';

describe('InventoryCard', () => {
  const defaultProps = {
    id: 1,
    productName: 'Producto Test',
    category: 'Electrónica',
    quantityOnHand: 50,
    quantityReserved: 10,
    availableQuantity: 40,
    reorderLevel: 20,
    status: 'OK',
  };

  it('renders product name and category', () => {
    render(<InventoryCard {...defaultProps} />);
    expect(screen.getByText('Producto Test')).toBeInTheDocument();
    expect(screen.getByText(/Electrónica/)).toBeInTheDocument();
  });

  it('displays stock quantities correctly', () => {
    render(<InventoryCard {...defaultProps} />);
    expect(screen.getByText('50')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument();
    expect(screen.getByText('40')).toBeInTheDocument();
    expect(screen.getByText('20')).toBeInTheDocument();
  });

  it('shows status badge with correct text', () => {
    render(<InventoryCard {...defaultProps} />);
    expect(screen.getByText('OK')).toBeInTheDocument();
  });

  it('applies correct CSS class for LOW status', () => {
    render(<InventoryCard {...defaultProps} status="LOW" />);
    const badge = screen.getByText('LOW');
    expect(badge.className).toContain('status-low');
  });

  it('applies correct CSS class for OUT_OF_STOCK status', () => {
    render(<InventoryCard {...defaultProps} status="OUT_OF_STOCK" />);
    const badge = screen.getByText('OUT_OF_STOCK');
    expect(badge.className).toContain('status-out');
  });

  it('renders adjust stock button', () => {
    render(<InventoryCard {...defaultProps} />);
    expect(screen.getByText('Ajustar stock')).toBeInTheDocument();
  });

  it('renders stock labels', () => {
    render(<InventoryCard {...defaultProps} />);
    expect(screen.getByText('En almacén')).toBeInTheDocument();
    expect(screen.getByText('Reservado')).toBeInTheDocument();
    expect(screen.getByText('Disponible')).toBeInTheDocument();
    expect(screen.getByText('Mínimo')).toBeInTheDocument();
  });
});
