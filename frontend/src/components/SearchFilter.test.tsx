import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { SearchFilter } from './SearchFilter';

describe('SearchFilter', () => {
  const defaultProps = {
    onSearch: vi.fn(),
    onFilter: vi.fn(),
    categories: ['Electrónica', 'Alimentos', 'Ropa'],
  };

  it('renders search input with placeholder', () => {
    render(<SearchFilter {...defaultProps} />);
    expect(screen.getByPlaceholderText(/Buscar producto/)).toBeInTheDocument();
  });

  it('renders category dropdown with all options', () => {
    render(<SearchFilter {...defaultProps} />);
    expect(screen.getByText('Todas las categorías')).toBeInTheDocument();
    expect(screen.getByText('Electrónica')).toBeInTheDocument();
    expect(screen.getByText('Alimentos')).toBeInTheDocument();
    expect(screen.getByText('Ropa')).toBeInTheDocument();
  });

  it('calls onSearch when typing in search input', async () => {
    const onSearch = vi.fn();
    const user = userEvent.setup();
    render(<SearchFilter {...defaultProps} onSearch={onSearch} />);

    await user.type(screen.getByPlaceholderText(/Buscar producto/), 'test');
    expect(onSearch).toHaveBeenCalled();
  });

  it('calls onFilter when selecting a category', async () => {
    const onFilter = vi.fn();
    const user = userEvent.setup();
    render(<SearchFilter {...defaultProps} onFilter={onFilter} />);

    await user.selectOptions(screen.getByRole('combobox'), 'Electrónica');
    expect(onFilter).toHaveBeenCalledWith('Electrónica');
  });
});
