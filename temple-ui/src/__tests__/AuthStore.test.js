import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useAuth } from '../stores/auth'
import axios from 'axios'

// Mock axios
vi.mock('axios')

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn()
}
global.localStorage = localStorageMock

describe('Auth Store', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorageMock.getItem.mockReturnValue(null)
  })

  describe('isAuthenticated', () => {
    it('returns false when no user or token', () => {
      const { isAuthenticated } = useAuth()
      expect(isAuthenticated.value).toBe(false)
    })

    it('returns true when user and token exist', async () => {
      const { login, isAuthenticated } = useAuth()
      
      axios.post.mockResolvedValue({
        data: {
          success: true,
          token: 'test-token',
          user: { id: 1, username: 'testuser', roles: ['Admin'] },
          permissions: ['ExpenseApproval']
        }
      })

      await login('testuser', 'password')
      expect(isAuthenticated.value).toBe(true)
    })
  })

  describe('hasRole', () => {
    it('returns false when user has no roles', () => {
      const { hasRole } = useAuth()
      expect(hasRole('Admin')).toBe(false)
    })

    it('returns true when user has the role', async () => {
      const { login, hasRole } = useAuth()
      
      axios.post.mockResolvedValue({
        data: {
          success: true,
          token: 'test-token',
          user: { id: 1, username: 'testuser', roles: ['Admin', 'Staff'] },
          permissions: []
        }
      })

      await login('testuser', 'password')
      expect(hasRole('Admin')).toBe(true)
      expect(hasRole('Staff')).toBe(true)
      expect(hasRole('Guest')).toBe(false)
    })
  })

  describe('hasPermission', () => {
    it('returns false when user has no permissions', () => {
      const { hasPermission } = useAuth()
      expect(hasPermission('ExpenseApproval')).toBe(false)
    })

    it('returns true when user has the permission', async () => {
      const { login, hasPermission } = useAuth()
      
      axios.post.mockResolvedValue({
        data: {
          success: true,
          token: 'test-token',
          user: { id: 1, username: 'testuser', roles: ['Staff'] },
          permissions: ['ExpenseApproval', 'UserManagement']
        }
      })

      await login('testuser', 'password')
      expect(hasPermission('ExpenseApproval')).toBe(true)
      expect(hasPermission('UserManagement')).toBe(true)
      expect(hasPermission('SystemConfig')).toBe(false)
    })
  })

  describe('login', () => {
    it('stores user data on successful login', async () => {
      const { login, user, token } = useAuth()
      
      const mockUser = { id: 1, username: 'testuser', roles: ['Admin'] }
      const mockToken = 'test-token-123'
      const mockPermissions = ['ExpenseApproval']
      
      axios.post.mockResolvedValue({
        data: {
          success: true,
          token: mockToken,
          user: mockUser,
          permissions: mockPermissions
        }
      })

      const result = await login('testuser', 'password')
      
      expect(result.success).toBe(true)
      expect(user.value).toEqual(mockUser)
      expect(token.value).toBe(mockToken)
      
      // Check localStorage calls
      expect(localStorageMock.setItem).toHaveBeenCalledWith('token', mockToken)
      expect(localStorageMock.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser))
      expect(localStorageMock.setItem).toHaveBeenCalledWith('permissions', JSON.stringify(mockPermissions))
      
      // Check axios defaults
      expect(axios.defaults.headers.common['Authorization']).toBe(`Bearer ${mockToken}`)
    })

    it('returns error on failed login', async () => {
      const { login } = useAuth()
      
      axios.post.mockResolvedValue({
        data: {
          success: false,
          message: 'Invalid credentials'
        }
      })

      const result = await login('testuser', 'wrongpassword')
      
      expect(result.success).toBe(false)
      expect(result.message).toBe('Invalid credentials')
    })
  })

  describe('logout', () => {
    it('clears all auth data', async () => {
      const { login, logout, user, token, isAuthenticated } = useAuth()
      
      // First login
      axios.post.mockResolvedValue({
        data: {
          success: true,
          token: 'test-token',
          user: { id: 1, username: 'testuser', roles: ['Admin'] },
          permissions: ['ExpenseApproval']
        }
      })

      await login('testuser', 'password')
      expect(isAuthenticated.value).toBe(true)

      // Mock window.location
      delete window.location
      window.location = { href: '', pathname: '/dashboard' }

      // Then logout
      logout()
      
      expect(user.value).toBe(null)
      expect(token.value).toBe(null)
      expect(isAuthenticated.value).toBe(false)
      
      // Check localStorage calls
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('token')
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('user')
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('permissions')
      
      // Check axios defaults cleared
      expect(axios.defaults.headers.common['Authorization']).toBeUndefined()
    })
  })

  describe('loadUser', () => {
    it('loads user data from localStorage', () => {
      const mockUser = { id: 1, username: 'testuser', roles: ['Admin'] }
      const mockToken = 'stored-token'
      const mockPermissions = ['ExpenseApproval']
      
      localStorageMock.getItem.mockImplementation((key) => {
        if (key === 'user') return JSON.stringify(mockUser)
        if (key === 'token') return mockToken
        if (key === 'permissions') return JSON.stringify(mockPermissions)
        return null
      })

      const { loadUser, user, token, isAuthenticated } = useAuth()
      loadUser()
      
      expect(user.value).toEqual(mockUser)
      expect(token.value).toBe(mockToken)
      expect(isAuthenticated.value).toBe(true)
      expect(axios.defaults.headers.common['Authorization']).toBe(`Bearer ${mockToken}`)
    })

    it('handles corrupted localStorage data', () => {
      localStorageMock.getItem.mockImplementation((key) => {
        if (key === 'user') return 'invalid-json'
        if (key === 'token') return 'token'
        return null
      })

      const { loadUser, isAuthenticated } = useAuth()
      
      // Should not throw
      expect(() => loadUser()).not.toThrow()
      expect(isAuthenticated.value).toBe(false)
    })
  })
})
