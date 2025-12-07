import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { FiLogOut } from 'react-icons/fi';
import { AdminSidebar } from '@/components/navigation';

interface AdminLayoutProps {
  children: ReactNode;
}

const AdminLayout = ({ children }: AdminLayoutProps) => {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top Bar */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex items-center justify-between px-6 py-4">
          <div className="flex items-center gap-4">
            <Link to="/" className="flex items-center gap-2">
              <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-xl">C</span>
              </div>
              <span className="text-xl font-bold text-gray-900">CarDealer</span>
            </Link>
            <div className="h-6 w-px bg-gray-300" />
            <span className="text-sm text-indigo-600 font-semibold">Portal Administrador</span>
          </div>

          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-600">Admin User</span>
            <Link
              to="/"
              className="flex items-center gap-2 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <FiLogOut className="w-4 h-4" />
              Salir
            </Link>
          </div>
        </div>
      </div>

      <div className="flex">
        {/* Sidebar */}
        <aside className="w-80 bg-white border-r border-gray-200 min-h-[calc(100vh-73px)] flex flex-col">
          <div className="p-4 flex-1 overflow-hidden">
            <AdminSidebar />
          </div>

          {/* Sidebar Footer */}
          <div className="p-4 border-t border-gray-100">
            <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-4">
              <p className="text-xs text-indigo-900 font-medium mb-1">Panel de Control</p>
              <p className="text-xs text-indigo-700">
                Gestiona dealers, publicidad y analítica de la plataforma.
              </p>
            </div>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 p-8 overflow-y-auto">
          {children}
        </main>
      </div>
    </div>
  );
};

export default AdminLayout;
