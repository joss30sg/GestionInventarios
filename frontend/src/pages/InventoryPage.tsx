import { useEffect, useState } from 'react';
import { Header } from '../components/Header';
import { SearchFilter } from '../components/SearchFilter';
import { InventoryCard } from '../components/InventoryCard';
import { inventoryApi } from '../services/api';
import './InventoryPage.css';

interface Inventory {
  id: number;
  productId: number;
  productName: string;
  category: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityOnOrder: number;
  availableQuantity: number;
  reorderLevel: number;
  status: string;
}

export function InventoryPage() {
  const [inventories, setInventories] = useState<Inventory[]>([]);
  const [filteredInventories, setFilteredInventories] = useState<Inventory[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');

  useEffect(() => {
    loadInventories();
  }, []);

  const loadInventories = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await inventoryApi.getAll();
      const responseData = response.data;
      const data: Inventory[] = Array.isArray(responseData?.data) 
        ? responseData.data 
        : Array.isArray(responseData) 
          ? responseData 
          : [];
      
      setInventories(data);
      
      const uniqueCategories = Array.from(
        new Set(data.map((item: Inventory) => item.category))
      ) as string[];
      setCategories(uniqueCategories.sort());
      
      filterInventories(data, searchTerm, selectedCategory);
    } catch (err: any) {
      setError('Error al cargar el inventario');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const filterInventories = (
    items: Inventory[],
    search: string,
    category: string
  ) => {
    let filtered = items;

    if (search) {
      filtered = filtered.filter((item) =>
        item.productName.toLowerCase().includes(search.toLowerCase())
      );
    }

    if (category) {
      filtered = filtered.filter((item) => item.category === category);
    }

    setFilteredInventories(filtered);
  };

  const handleSearch = (term: string) => {
    setSearchTerm(term);
    filterInventories(inventories, term, selectedCategory);
  };

  const handleFilter = (category: string) => {
    setSelectedCategory(category);
    filterInventories(inventories, searchTerm, category);
  };

  return (
    <div className="inventory-page">
      <Header />

      <main className="page-container">
        <div className="page-header">
          <h2>Gestión de Inventarios</h2>
        </div>

        {error && <div className="error-alert">{error}</div>}

        <SearchFilter
          onSearch={handleSearch}
          onFilter={handleFilter}
          categories={categories}
        />

        {loading ? (
          <div className="loading">
            <div className="spinner"></div>
            <p>Cargando inventarios...</p>
          </div>
        ) : filteredInventories.length === 0 ? (
          <div className="empty-state">
            <p>📭 No se encontraron productos</p>
          </div>
        ) : (
          <div className="inventory-grid">
            {filteredInventories.map((inventory) => (
              <InventoryCard
                key={inventory.id}
                id={inventory.id}
                productName={inventory.productName}
                category={inventory.category}
                quantityOnHand={inventory.quantityOnHand}
                quantityReserved={inventory.quantityReserved}
                availableQuantity={inventory.availableQuantity}
                reorderLevel={inventory.reorderLevel}
                status={inventory.status}
              />
            ))}
          </div>
        )}

        <div className="page-footer">
          <p>
            {filteredInventories.length} de {inventories.length} productos mostrados
          </p>
        </div>
      </main>
    </div>
  );
}
