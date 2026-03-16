import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Header } from '../components/Header';
import { SearchFilter } from '../components/SearchFilter';
import { useAuth } from '../context/AuthContext';
import { productsApi } from '../services/api';
import { Product } from '../types';
import './ProductsPage.css';

interface ProductForm {
  name: string;
  description: string;
  price: string;
  quantity: string;
  category: string;
}

const emptyForm: ProductForm = { name: '', description: '', price: '', quantity: '', category: '' };

export function ProductsPage() {
  const { isAdmin } = useAuth();
  const navigate = useNavigate();
  const [products, setProducts] = useState<Product[]>([]);
  const [filteredProducts, setFilteredProducts] = useState<Product[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<ProductForm>(emptyForm);
  const [formError, setFormError] = useState('');
  const [saving, setSaving] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(5);

  useEffect(() => { loadProducts(); }, []);

  const loadProducts = async () => {
    try {
      setLoading(true);
      const res = await productsApi.getAll();
      const data: Product[] = res.data.data || [];
      data.sort((a, b) => a.id - b.id);
      setProducts(data);
      filterProducts(data, searchTerm, selectedCategory);
    } catch {
      setError('Error al cargar productos');
    } finally {
      setLoading(false);
    }
  };

  const filterProducts = (items: Product[], search: string, category: string) => {
    let filtered = items;
    if (search) {
      filtered = filtered.filter(p => p.name.toLowerCase().includes(search.toLowerCase()));
    }
    if (category) {
      filtered = filtered.filter(p => p.category === category);
    }
    setFilteredProducts(filtered);
    setCurrentPage(1);
  };

  const totalPages = Math.ceil(filteredProducts.length / itemsPerPage);
  const paginatedProducts = filteredProducts.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  const handleSearch = (term: string) => {
    setSearchTerm(term);
    filterProducts(products, term, selectedCategory);
  };

  const handleFilter = (category: string) => {
    setSelectedCategory(category);
    filterProducts(products, searchTerm, category);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const openCreate = () => {
    setForm(emptyForm);
    setEditingId(null);
    setFormError('');
    setShowForm(true);
  };

  const openEdit = (p: Product) => {
    setForm({
      name: p.name,
      description: p.description,
      price: p.price.toString(),
      quantity: p.quantity.toString(),
      category: p.category,
    });
    setEditingId(p.id);
    setFormError('');
    setShowForm(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    const price = parseFloat(form.price);
    const quantity = parseInt(form.quantity, 10);

    if (!form.name.trim()) { setFormError('El nombre es requerido'); return; }
    if (!form.category.trim()) { setFormError('La categoría es requerida'); return; }
    if (isNaN(price) || price <= 0) { setFormError('El precio debe ser mayor a 0'); return; }
    if (isNaN(quantity) || quantity < 0) { setFormError('La cantidad no puede ser negativa'); return; }

    const data = {
      name: form.name.trim(),
      description: form.description.trim(),
      price,
      quantity,
      category: form.category.trim(),
    };

    try {
      setSaving(true);
      if (editingId) {
        await productsApi.update(editingId, data);
      } else {
        await productsApi.create(data);
      }
      setShowForm(false);
      await loadProducts();
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data?.errors?.[0]?.message || 'Error al guardar producto';
      setFormError(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number, name: string) => {
    if (!window.confirm(`¿Eliminar "${name}"?`)) return;
    try {
      await productsApi.delete(id);
      await loadProducts();
    } catch {
      setError('Error al eliminar producto');
    }
  };

  const categories = Array.from(new Set(products.map(p => p.category))).sort();

  return (
    <div className="products-page">
      <Header />
      <main className="page-container">
        <div className="page-header">
          <h2>📦 Productos</h2>
          <div className="page-header-actions">
            {isAdmin && (
              <button className="btn-primary" onClick={openCreate}>+ Nuevo Producto</button>
            )}
            <button className="btn-back" onClick={() => navigate('/inventory')}>← Regresar al Inicio</button>
          </div>
        </div>

        {error && <div className="error-alert">{error}</div>}

        <SearchFilter
          onSearch={handleSearch}
          onFilter={handleFilter}
          categories={categories}
        />

        {showForm && (
          <div className="modal-overlay" onClick={() => setShowForm(false)}>
            <div className="modal-content" onClick={e => e.stopPropagation()}>
              <h3>{editingId ? 'Editar Producto' : 'Nuevo Producto'}</h3>
              <form onSubmit={handleSubmit} className="product-form">
                {formError && <div className="error-message">{formError}</div>}
                <div className="form-group">
                  <label htmlFor="name">Nombre</label>
                  <input id="name" name="name" value={form.name} onChange={handleChange} placeholder="Nombre del producto" required />
                </div>
                <div className="form-group">
                  <label htmlFor="description">Descripción</label>
                  <textarea id="description" name="description" value={form.description} onChange={handleChange} placeholder="Descripción del producto" rows={3} />
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="price">Precio (S/.)</label>
                    <input id="price" name="price" type="number" step="0.01" min="0.01" value={form.price} onChange={handleChange} placeholder="0.00" required />
                  </div>
                  <div className="form-group">
                    <label htmlFor="quantity">Cantidad</label>
                    <input id="quantity" name="quantity" type="number" min="0" step="1" value={form.quantity} onChange={handleChange} placeholder="0" required />
                  </div>
                </div>
                <div className="form-group">
                  <label htmlFor="category">Categoría</label>
                  <input id="category" name="category" value={form.category} onChange={handleChange} placeholder="Ej: Electrónica, Mobiliario" required list="category-list" />
                  <datalist id="category-list">
                    {categories.map(c => <option key={c} value={c} />)}
                  </datalist>
                </div>
                <div className="form-actions">
                  <button type="button" className="btn-secondary" onClick={() => setShowForm(false)} disabled={saving}>Cancelar</button>
                  <button type="submit" className="btn-primary" disabled={saving}>{saving ? 'Guardando...' : editingId ? 'Actualizar' : 'Crear'}</button>
                </div>
              </form>
            </div>
          </div>
        )}

        {loading ? (
          <p className="loading-text">Cargando productos...</p>
        ) : filteredProducts.length === 0 ? (
          <p className="empty-text">No hay productos que coincidan con la búsqueda.</p>
        ) : (
          <div className="products-table-container">
            <table className="products-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nombre</th>
                  <th>Descripción</th>
                  <th>Precio (S/.)</th>
                  <th>Cantidad</th>
                  <th>Categoría</th>
                  {isAdmin && <th>Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {paginatedProducts.map(p => (
                  <tr key={p.id} className={p.quantity === 0 ? 'row-out-of-stock' : p.quantity < 5 ? 'row-low-stock' : ''}>
                    <td>{p.id}</td>
                    <td className="td-name">{p.name}</td>
                    <td className="td-desc">{p.description}</td>
                    <td className="td-price">S/. {p.price.toFixed(2)}</td>
                    <td className={`td-qty ${p.quantity === 0 ? 'qty-zero' : p.quantity < 5 ? 'qty-low' : ''}`}>{p.quantity}</td>
                    <td><span className="category-badge">{p.category}</span></td>
                    {isAdmin && (
                      <td className="td-actions">
                        <button className="btn-edit" onClick={() => openEdit(p)} title="Editar">✏️</button>
                        <button className="btn-delete" onClick={() => handleDelete(p.id, p.name)} title="Eliminar">🗑️</button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {!loading && filteredProducts.length > 0 && (
          <div className="pagination">
            <div className="pagination-info">
              Mostrando {(currentPage - 1) * itemsPerPage + 1}-{Math.min(currentPage * itemsPerPage, filteredProducts.length)} de {filteredProducts.length} productos
            </div>
            <div className="pagination-controls">
              <button
                className="pagination-btn"
                onClick={() => setCurrentPage(1)}
                disabled={currentPage === 1}
              >
                «
              </button>
              <button
                className="pagination-btn"
                onClick={() => setCurrentPage(p => p - 1)}
                disabled={currentPage === 1}
              >
                ‹
              </button>
              {Array.from({ length: totalPages }, (_, i) => i + 1)
                .filter(page => page === 1 || page === totalPages || Math.abs(page - currentPage) <= 1)
                .map((page, idx, arr) => (
                  <span key={page}>
                    {idx > 0 && arr[idx - 1] !== page - 1 && <span className="pagination-ellipsis">...</span>}
                    <button
                      className={`pagination-btn ${currentPage === page ? 'active' : ''}`}
                      onClick={() => setCurrentPage(page)}
                    >
                      {page}
                    </button>
                  </span>
                ))}
              <button
                className="pagination-btn"
                onClick={() => setCurrentPage(p => p + 1)}
                disabled={currentPage === totalPages}
              >
                ›
              </button>
              <button
                className="pagination-btn"
                onClick={() => setCurrentPage(totalPages)}
                disabled={currentPage === totalPages}
              >
                »
              </button>
              <select
                className="pagination-select"
                value={itemsPerPage}
                onChange={e => { setItemsPerPage(Number(e.target.value)); setCurrentPage(1); }}
              >
                <option value={5}>5 / pág</option>
                <option value={10}>10 / pág</option>
                <option value={20}>20 / pág</option>
              </select>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
