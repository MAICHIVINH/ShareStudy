import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

const SettingsContext = createContext();

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5068/api';

export const SettingsProvider = ({ children }) => {
  const [settings, setSettings] = useState({
    siteName: 'ShareStudy - Chia sẻ tài liệu học tập',
    siteSlogan: 'Tri thức mở, kết nối cộng đồng học thuật Việt Nam',
    contactEmail: 'support@sharestudy.vn',
    maintenanceMode: false,
    allowRegistration: true,
    requireEmailVerification: true,
    maxUploadMB: 50,
    allowedFileExtensions: ['pdf', 'docx', 'pptx', 'xlsx', 'zip'],
  });

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchPublicSettings();
  }, []);

  const fetchPublicSettings = async () => {
    try {
      const res = await axios.get(`${API_URL}/settings/public`);
      if (res.data?.data) {
        setSettings((prev) => ({
          ...prev,
          ...res.data.data,
        }));
      }
    } catch (err) {
      console.log('Using default public settings fallback');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SettingsContext.Provider value={{ settings, refreshSettings: fetchPublicSettings, loading }}>
      {children}
    </SettingsContext.Provider>
  );
};

export const useSettings = () => useContext(SettingsContext);
