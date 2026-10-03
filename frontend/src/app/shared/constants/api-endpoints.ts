export const ApiEndpoints = {
  Auth: {
    BASE: '/api/v1/Auth',
    LOGIN: '/login',
    LOGOUT: '/logout',
    REGISTER: '/register',
    REFRESH_TOKEN: '/renew-token',
    FORGOT_PASSWORD: '/forgot-password',
    RESET_PASSWORD: '/reset-password',
    REQUEST_ACCESS: '/request-access'
  },
  Account: {
    BASE: '/api/v1/Account',
    CHANGE_PASSWORD: '/change-password',
    PROFILE: '/profile'
  },
  User: {
    BASE: '/api/v1/User',
    LIST: '',
    DETAIL: '/:id',
    LOOKUP_LIST: '/lookup-list',
    ENABLE_DISABLE: '/enable-disable'
  },
  Role: {
    BASE: '/api/v1/Role',
    LIST: '',
    DETAIL: '/:id',
    LOOKUP_LIST: '/lookup-list'
  },
  Setting: {
    BASE: '/api/v1/Setting',
    LIST: '',
    DETAIL: '/:id'
  },
  Project: {
    BASE: '/api/v1/Project',
    LIST: '',
    DETAIL: '/:id',
    LOOKUP_LIST: '/lookup-list'
  },
  Task: {
    BASE: '/api/v1/Task',
    LIST: '',
    DETAIL: '/:id',
    LOOKUP_LIST: '/lookup-list'
  },
  AuditLog: {
    BASE: '/api/v1/AuditLog',
    LIST: ''
  }
} as const;

export type ApiEndpoint = typeof ApiEndpoints.Auth;