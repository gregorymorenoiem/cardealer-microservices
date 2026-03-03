/* eslint-disable @next/next/no-img-element */
import { ImageResponse } from 'next/og';

export const runtime = 'edge';
export const alt = 'OKLA - Marketplace de Vehículos #1 en República Dominicana';
export const size = { width: 1200, height: 630 };
export const contentType = 'image/png';

export default async function Image() {
  return new ImageResponse(
    <div
      style={{
        background: 'linear-gradient(135deg, #00A870 0%, #009663 50%, #007a4f 100%)',
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        fontFamily: 'system-ui, -apple-system, sans-serif',
      }}
    >
      {/* Logo */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: '20px',
          marginBottom: '30px',
        }}
      >
        <div
          style={{
            width: '90px',
            height: '90px',
            background: 'white',
            borderRadius: '24px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '54px',
            fontWeight: 'bold',
            color: '#00A870',
          }}
        >
          O
        </div>
        <span
          style={{
            fontSize: '72px',
            fontWeight: 'bold',
            color: 'white',
            letterSpacing: '-2px',
          }}
        >
          OKLA
        </span>
      </div>

      {/* Tagline */}
      <div
        style={{
          fontSize: '32px',
          color: 'rgba(255, 255, 255, 0.95)',
          fontWeight: '600',
          textAlign: 'center',
          maxWidth: '800px',
        }}
      >
        Marketplace de Vehículos #1
      </div>
      <div
        style={{
          fontSize: '28px',
          color: 'rgba(255, 255, 255, 0.8)',
          fontWeight: '400',
          textAlign: 'center',
          marginTop: '8px',
        }}
      >
        en República Dominicana 🇩🇴
      </div>

      {/* Stats */}
      <div
        style={{
          display: 'flex',
          gap: '60px',
          marginTop: '50px',
        }}
      >
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <span style={{ fontSize: '36px', fontWeight: 'bold', color: 'white' }}>🚗</span>
          <span style={{ fontSize: '18px', color: 'rgba(255,255,255,0.9)', marginTop: '4px' }}>
            Miles de vehículos
          </span>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <span style={{ fontSize: '36px', fontWeight: 'bold', color: 'white' }}>⭐</span>
          <span style={{ fontSize: '18px', color: 'rgba(255,255,255,0.9)', marginTop: '4px' }}>
            OKLA Score™
          </span>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <span style={{ fontSize: '36px', fontWeight: 'bold', color: 'white' }}>🔒</span>
          <span style={{ fontSize: '18px', color: 'rgba(255,255,255,0.9)', marginTop: '4px' }}>
            Compra segura
          </span>
        </div>
      </div>

      {/* URL */}
      <div
        style={{
          position: 'absolute',
          bottom: '30px',
          fontSize: '20px',
          color: 'rgba(255, 255, 255, 0.6)',
        }}
      >
        okla.com.do
      </div>
    </div>,
    { ...size }
  );
}
