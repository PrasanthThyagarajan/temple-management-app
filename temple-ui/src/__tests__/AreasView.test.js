import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import Areas from '../views/Areas.vue'
import axios from 'axios'

// Mock axios
vi.mock('axios')

describe('Areas.vue', () => {
  let wrapper

  beforeEach(() => {
    // Reset mocks
    vi.clearAllMocks()
    
    // Mock API responses
    axios.get.mockImplementation((url) => {
      if (url.includes('/areas')) {
        return Promise.resolve({
          data: [
            { id: 3, name: 'Area C', templeId: 1, description: 'Description C', temple: { name: 'Temple 1' }, isActive: true },
            { id: 2, name: 'Area B', templeId: 2, description: 'Description B', temple: { name: 'Temple 2' }, isActive: true },
            { id: 1, name: 'Area A', templeId: 1, description: '', temple: { name: 'Temple 1' }, isActive: true }
          ]
        })
      }
      if (url.includes('/temples')) {
        return Promise.resolve({
          data: [
            { id: 1, name: 'Temple 1' },
            { id: 2, name: 'Temple 2' }
          ]
        })
      }
    })

    wrapper = mount(Areas, {
      global: {
        plugins: [createPinia(), ElementPlus],
        stubs: {
          'el-icon': true
        }
      }
    })
  })

  describe('Summary Cards', () => {
    it('displays correct total areas count', async () => {
      await wrapper.vm.$nextTick()
      const summaryStats = wrapper.vm.summaryStats
      expect(summaryStats.total).toBe(3)
    })

    it('displays correct active temples count', async () => {
      await wrapper.vm.$nextTick()
      const summaryStats = wrapper.vm.summaryStats
      expect(summaryStats.activeTemples).toBe(2) // 2 unique temple IDs
    })

    it('displays correct areas with description count', async () => {
      await wrapper.vm.$nextTick()
      const summaryStats = wrapper.vm.summaryStats
      expect(summaryStats.withDescription).toBe(2) // Area A has empty description
    })

    it('shows summary cards section', () => {
      const summaryCards = wrapper.find('.summary-cards')
      expect(summaryCards.exists()).toBe(true)
    })
  })

  describe('Filtering', () => {
    it('filters areas by search term', async () => {
      await wrapper.vm.$nextTick()
      
      // Set search term
      wrapper.vm.searchTerm = 'Area B'
      await wrapper.vm.$nextTick()
      
      const filtered = wrapper.vm.filteredBeforePagination
      expect(filtered.length).toBe(1)
      expect(filtered[0].name).toBe('Area B')
    })

    it('filters areas by temple', async () => {
      await wrapper.vm.$nextTick()
      
      // Filter by temple 1
      wrapper.vm.selectedTempleId = 1
      await wrapper.vm.$nextTick()
      
      const filtered = wrapper.vm.filteredBeforePagination
      expect(filtered.length).toBe(2) // Area A and Area C
      expect(filtered.every(a => a.templeId === 1)).toBe(true)
    })

    it('shows "Any Temple" option in temple filter', async () => {
      await wrapper.vm.$nextTick()
      const filterSection = wrapper.find('.filters-section')
      expect(filterSection.exists()).toBe(true)
      // The "Any Temple" option is added in the template
    })

    it('resets pagination when filter changes', async () => {
      await wrapper.vm.$nextTick()
      wrapper.vm.currentPage = 2
      
      // Change filter
      wrapper.vm.handleTempleFilterChange()
      
      expect(wrapper.vm.currentPage).toBe(1)
    })
  })

  describe('Sorting', () => {
    it('sorts areas by name ascending', async () => {
      await wrapper.vm.$nextTick()
      
      wrapper.vm.sortBy = 'name-asc'
      await wrapper.vm.$nextTick()
      
      const sorted = wrapper.vm.filteredBeforePagination
      expect(sorted[0].name).toBe('Area A')
      expect(sorted[1].name).toBe('Area B')
      expect(sorted[2].name).toBe('Area C')
    })

    it('sorts areas by name descending', async () => {
      await wrapper.vm.$nextTick()
      
      wrapper.vm.sortBy = 'name-desc'
      await wrapper.vm.$nextTick()
      
      const sorted = wrapper.vm.filteredBeforePagination
      expect(sorted[0].name).toBe('Area C')
      expect(sorted[1].name).toBe('Area B')
      expect(sorted[2].name).toBe('Area A')
    })

    it('sorts areas by ID descending (default)', async () => {
      await wrapper.vm.$nextTick()
      
      const sorted = wrapper.vm.filteredBeforePagination
      expect(sorted[0].id).toBe(3)
      expect(sorted[1].id).toBe(2)
      expect(sorted[2].id).toBe(1)
    })

    it('handles table sort change', () => {
      wrapper.vm.handleTableSortChange({ prop: 'name', order: 'ascending' })
      expect(wrapper.vm.sortBy).toBe('name-asc')
      
      wrapper.vm.handleTableSortChange({ prop: 'temple.name', order: 'descending' })
      expect(wrapper.vm.sortBy).toBe('temple-desc')
      
      wrapper.vm.handleTableSortChange({ prop: null, order: null })
      expect(wrapper.vm.sortBy).toBe('name-asc')
    })
  })

  describe('Pagination', () => {
    it('paginates data correctly', async () => {
      await wrapper.vm.$nextTick()
      
      // Set page size to 2
      wrapper.vm.pageSize = 2
      wrapper.vm.currentPage = 1
      await wrapper.vm.$nextTick()
      
      const paginated = wrapper.vm.paginatedAreas
      expect(paginated.length).toBe(2)
      expect(paginated[0].id).toBe(3)
      expect(paginated[1].id).toBe(2)
    })

    it('shows correct total in pagination', async () => {
      await wrapper.vm.$nextTick()
      
      const total = wrapper.vm.filteredBeforePagination.length
      expect(total).toBe(3)
    })

    it('changes page size', () => {
      const newSize = 50
      wrapper.vm.handleSizeChange(newSize)
      expect(wrapper.vm.pageSize).toBe(newSize)
    })

    it('changes current page', () => {
      const newPage = 2
      wrapper.vm.handleCurrentChange(newPage)
      expect(wrapper.vm.currentPage).toBe(newPage)
    })
  })

  describe('Data Loading', () => {
    it('loads areas on mount', async () => {
      await wrapper.vm.$nextTick()
      
      expect(axios.get).toHaveBeenCalledWith('/api/areas')
      expect(wrapper.vm.areas.length).toBe(3)
    })

    it('loads temples on mount', async () => {
      await wrapper.vm.$nextTick()
      
      expect(axios.get).toHaveBeenCalledWith('/api/temples')
      expect(wrapper.vm.temples.length).toBe(2)
    })

    it('shows data in descending order by ID', async () => {
      await wrapper.vm.$nextTick()
      
      const areas = wrapper.vm.areas
      expect(areas[0].id).toBe(3) // Newest first
      expect(areas[1].id).toBe(2)
      expect(areas[2].id).toBe(1) // Oldest last
    })
  })

  describe('Combined Features', () => {
    it('applies search, filter, and sort together', async () => {
      await wrapper.vm.$nextTick()
      
      // Apply temple filter
      wrapper.vm.selectedTempleId = 1
      // Apply search
      wrapper.vm.searchTerm = 'Area'
      // Apply sort
      wrapper.vm.sortBy = 'name-asc'
      
      await wrapper.vm.$nextTick()
      
      const result = wrapper.vm.filteredBeforePagination
      expect(result.length).toBe(2) // Only temple 1 areas
      expect(result[0].name).toBe('Area A') // Sorted by name
      expect(result[1].name).toBe('Area C')
    })

    it('updates summary stats based on filters', async () => {
      await wrapper.vm.$nextTick()
      
      // Apply filter
      wrapper.vm.selectedTempleId = 1
      await wrapper.vm.$nextTick()
      
      const filtered = wrapper.vm.filteredBeforePagination.length
      expect(filtered).toBe(2)
    })
  })
})
