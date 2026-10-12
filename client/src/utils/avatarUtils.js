export const formatAvatarUrl = (url, name = 'User') => {
  if (!url || typeof url !== 'string' || !url.trim()) {
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=2563eb&color=fff`;
  }

  const cleanUrl = url.trim();
  const apiBase = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
  const apiDomain = apiBase.replace(/\/api\/?$/, ''); // e.g. http://localhost:5000

  // Replace any old localhost:5068 port with current active API domain
  if (cleanUrl.includes('localhost:5068')) {
    return cleanUrl.replace('http://localhost:5068', apiDomain);
  }

  // If old MinIO direct URL (:9000), rewrite to API Storage Proxy
  if (cleanUrl.includes(':9000/documents/')) {
    const objectPath = cleanUrl.split(':9000/documents/')[1];
    return `${apiDomain}/api/storage/${objectPath}`;
  }

  // If relative path like /api/storage/
  if (cleanUrl.startsWith('/api/storage/')) {
    return `${apiDomain}${cleanUrl}`;
  }

  return cleanUrl;
};
