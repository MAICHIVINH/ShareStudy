import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

export const LoginPage = () => {
  const navigate = useNavigate();
  const { openLoginModal } = useAuth();

  useEffect(() => {
    openLoginModal();
    navigate('/', { replace: true });
  }, [openLoginModal, navigate]);

  return null;
};

export default LoginPage;
