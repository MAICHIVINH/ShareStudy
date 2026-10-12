import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

export const RegisterPage = () => {
  const navigate = useNavigate();
  const { openRegisterModal } = useAuth();

  useEffect(() => {
    openRegisterModal();
    navigate('/', { replace: true });
  }, [openRegisterModal, navigate]);

  return null;
};

export default RegisterPage;
