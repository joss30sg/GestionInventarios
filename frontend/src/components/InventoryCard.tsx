import './InventoryCard.css';

interface InventoryCardProps {
  id: number;
  productName: string;
  category: string;
  quantityOnHand: number;
  quantityReserved: number;
  availableQuantity: number;
  reorderLevel: number;
  status: string;
}

export function InventoryCard({
  productName,
  category,
  quantityOnHand,
  quantityReserved,
  availableQuantity,
  reorderLevel,
  status,
}: InventoryCardProps) {
  const getStatusColor = (st: string) => {
    switch (st.toUpperCase()) {
      case 'OK':
        return 'status-ok';
      case 'LOW':
        return 'status-low';
      case 'OUT_OF_STOCK':
        return 'status-out';
      default:
        return 'status-ok';
    }
  };

  return (
    <div className="inventory-card">
      <div className="card-header">
        <h3 className="product-name">{productName}</h3>
        <span className={`status-badge ${getStatusColor(status)}`}>
          {status}
        </span>
      </div>

      <div className="card-body">
        <div className="category-icon">
          📁 {category}
        </div>

        <div className="stock-grid">
          <div className="stock-item">
            <span className="stock-label">En almacén</span>
            <span className="stock-value">{quantityOnHand}</span>
          </div>

          <div className="stock-item">
            <span className="stock-label">Reservado</span>
            <span className="stock-value warning">{quantityReserved}</span>
          </div>

          <div className="stock-item">
            <span className="stock-label">Disponible</span>
            <span className="stock-value success">{availableQuantity}</span>
          </div>

          <div className="stock-item">
            <span className="stock-label">Mínimo</span>
            <span className="stock-value">{reorderLevel}</span>
          </div>
        </div>

        <div className="progress-bar">
          <div
            className="progress-fill"
            style={{
              width: `${Math.min((availableQuantity / (reorderLevel * 2)) * 100, 100)}%`,
            }}
          />
        </div>
      </div>

      <div className="card-footer">
        <button className="btn-adjust">Ajustar stock</button>
      </div>
    </div>
  );
}
