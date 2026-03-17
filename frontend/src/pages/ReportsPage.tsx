import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';
import './ReportsPage.css';
import { API_CONFIG } from '../constants';

interface LowStockProduct {
  productId: number;
  productName: string;
  category: string;
  currentQuantity: number;
  reorderLevel: number;
  unitPrice: number;
  totalValue: number;
  status: 'Critical' | 'Warning' | 'Low';
  lastUpdated: string;
}

interface ReportSummary {
  totalProductsLowStock: number;
  criticalCount: number;
  warningCount: number;
  lowCount: number;
  totalValue: number;
  generatedDate: string;
  generatedBy: string;
}

export const ReportsPage: React.FC = () => {
  const { token, user } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [pdfLoading, setPdfLoading] = useState(false);
  const [products, setProducts] = useState<LowStockProduct[]>([]);
  const [summary, setSummary] = useState<ReportSummary | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Obtener productos con inventario bajo
  const fetchLowStockProducts = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await axios.get<{
        success: boolean;
        data: LowStockProduct[];
        count: number;
      }>(`${API_CONFIG.baseURL}/api/reports/low-stock`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.data.success) {
        setProducts(response.data.data);
        setError(null);
      }
    } catch (err: any) {
      const errorMsg =
        err.response?.data?.message || 'Error al cargar los productos';
      setError(errorMsg);
      console.error('Error al obtener productos:', err);
    } finally {
      setLoading(false);
    }
  };

  // Obtener resumen del reporte
  const fetchReportSummary = async () => {
    try {
      const response = await axios.get<{
        success: boolean;
        data: ReportSummary;
      }>(`${API_CONFIG.baseURL}/api/reports/summary`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.data.success) {
        setSummary(response.data.data);
      }
    } catch (err) {
      console.error('Error al obtener resumen:', err);
    }
  };

  // Descargar PDF
  const downloadPdfReport = async () => {
    setPdfLoading(true);
    setError(null);

    try {
      const response = await axios.get(`${API_CONFIG.baseURL}/api/reports/low-stock-pdf`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
        responseType: 'blob',
      });

      // Crear un Blob y descargarlo
      const blob = new Blob([response.data], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `ReporteInventarioBajo_${new Date().toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      setSuccessMessage('✓ PDF descargado exitosamente');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      const errorMsg =
        err.response?.data?.message || 'Error al descargar el PDF';
      setError(errorMsg);
      console.error('Error al descargar PDF:', err);
    } finally {
      setPdfLoading(false);
    }
  };

  // Cargar datos al montar
  useEffect(() => {
    if (token) {
      fetchLowStockProducts();
      fetchReportSummary();
    }
  }, [token]);

  if (!user || user.role !== 'Admin') {
    return (
      <div className="reports-unauthorized">
        <h2>Acceso Denegado</h2>
        <p>Solo los administradores pueden acceder a los reportes.</p>
      </div>
    );
  }

  return (
    <div className="reports-container">
      <div className="reports-header">
        <h1>📊 Módulo de Reportes</h1>
        <p>Genera y descarga reportes de productos con inventario bajo</p>
      </div>

      {/* Summary Cards */}
      {summary && (
        <div className="reports-summary">
          <div className="summary-card summary-card-critical">
            <div className="summary-icon">🚨</div>
            <div className="summary-content">
              <span className="summary-label">Críticos</span>
              <span className="summary-value">{summary.criticalCount}</span>
            </div>
          </div>

          <div className="summary-card summary-card-warning">
            <div className="summary-icon">⚠️</div>
            <div className="summary-content">
              <span className="summary-label">Advertencia</span>
              <span className="summary-value">{summary.warningCount}</span>
            </div>
          </div>

          <div className="summary-card summary-card-low">
            <div className="summary-icon">📉</div>
            <div className="summary-content">
              <span className="summary-label">Bajos</span>
              <span className="summary-value">{summary.lowCount}</span>
            </div>
          </div>

          <div className="summary-card summary-card-total">
            <div className="summary-icon">📦</div>
            <div className="summary-content">
              <span className="summary-label">Total Items</span>
              <span className="summary-value">{summary.totalProductsLowStock}</span>
            </div>
          </div>

          <div className="summary-card summary-card-value">
            <div className="summary-icon">💰</div>
            <div className="summary-content">
              <span className="summary-label">Valor Total</span>
              <span className="summary-value">S/. {summary.totalValue.toFixed(2)}</span>
            </div>
          </div>
        </div>
      )}

      {/* Action Buttons */}
      <div className="reports-actions">
        <button
          className="btn-primary"
          onClick={() => { fetchLowStockProducts(); fetchReportSummary(); }}
          disabled={loading}
        >
          {loading ? '⏳ Cargando...' : '🔄 Actualizar Datos'}
        </button>

        <button
          className="btn-success"
          onClick={downloadPdfReport}
          disabled={pdfLoading || products.length === 0}
        >
          {pdfLoading ? '⏳ Generando PDF...' : '📥 Descargar Reporte PDF'}
        </button>

        <button
          className="btn-secondary"
          onClick={() => navigate('/inventory')}
        >
          ← Regresar al Inicio
        </button>
      </div>

      {/* Messages */}
      {error && <div className="alert alert-error">{error}</div>}
      {successMessage && <div className="alert alert-success">{successMessage}</div>}

      {/* Products Table */}
      <div className="reports-table-container">
        <h2>Productos con Inventario Bajo</h2>

        {products.length === 0 ? (
          <div className="no-data">
            <p>✓ ¡Excelente! No hay productos con inventario bajo.</p>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="reports-table">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th>Categoría</th>
                  <th>Cantidad Actual</th>
                  <th>Mínimo</th>
                  <th>Precio Unit.</th>
                  <th>Valor Total</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                {products.map((product) => (
                  <tr key={product.productId} className={`status-${product.status.toLowerCase()}`}>
                    <td className="product-name">{product.productName}</td>
                    <td>{product.category}</td>
                    <td className="quantity-cell">{product.currentQuantity}</td>
                    <td>{product.reorderLevel}</td>
                    <td>S/. {product.unitPrice.toFixed(2)}</td>
                    <td className="value-cell">S/. {product.totalValue.toFixed(2)}</td>
                    <td>
                      <span className={`status-badge status-${product.status.toLowerCase()}`}>
                        {product.status === 'Critical' && '🚨'}
                        {product.status === 'Warning' && '⚠️'}
                        {product.status === 'Low' && '📉'}
                        {' '}
                        {product.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Report Info */}
      {summary && (
        <div className="report-info">
          <p>
            <strong>Generado:</strong> {new Date(summary.generatedDate).toLocaleString()}
          </p>
          <p>
            <strong>Generado por:</strong> {summary.generatedBy}
          </p>
        </div>
      )}
    </div>
  );
};

export default ReportsPage;
