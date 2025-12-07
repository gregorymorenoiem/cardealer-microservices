import { FiPrinter } from 'react-icons/fi';

export default function PrintButton() {
  const handlePrint = () => {
    window.print();
  };

  return (
    <button
      onClick={handlePrint}
      className="flex items-center gap-2 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-lg hover:border-primary hover:text-primary transition-all font-medium print:hidden"
      aria-label="Print vehicle details"
    >
      <FiPrinter size={20} />
      Print
    </button>
  );
}
