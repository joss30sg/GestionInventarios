import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { NotFound } from './NotFound';

describe('NotFound', () => {
  it('renders 404 code', () => {
    render(<NotFound />);
    expect(screen.getByText('404')).toBeInTheDocument();
  });

  it('renders error message', () => {
    render(<NotFound />);
    expect(screen.getByText('Página no encontrada')).toBeInTheDocument();
    expect(screen.getByText('La página que buscas no existe.')).toBeInTheDocument();
  });

  it('renders link to home', () => {
    render(<NotFound />);
    const link = screen.getByText('Volver al inicio');
    expect(link).toBeInTheDocument();
    expect(link.getAttribute('href')).toBe('/');
  });
});
