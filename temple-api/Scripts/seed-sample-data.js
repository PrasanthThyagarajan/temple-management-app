const axios = require('axios');

const API_BASE = 'http://localhost:5051/api';

async function seedSampleData() {
  try {
    console.log('🌱 Seeding sample data...');

    // Create sample products
    console.log('Creating sample products...');
    const products = [
      { name: 'Incense Sticks', category: 'Pooja Items', categoryId: null, stockQuantity: 100, minStockLevel: 10, price: 25.00, status: 'Active', description: 'Fragrant incense sticks for prayers', notes: '' },
      { name: 'Coconut', category: 'Offerings', categoryId: null, stockQuantity: 50, minStockLevel: 5, price: 30.00, status: 'Active', description: 'Fresh coconut for offerings', notes: '' },
      { name: 'Flowers Garland', category: 'Decorations', categoryId: null, stockQuantity: 25, minStockLevel: 3, price: 50.00, status: 'Active', description: 'Beautiful flower garlands', notes: '' },
      { name: 'Camphor', category: 'Pooja Items', categoryId: null, stockQuantity: 75, minStockLevel: 8, price: 15.00, status: 'Active', description: 'Pure camphor for aarti', notes: '' },
      { name: 'Prasadam Box', category: 'Food Items', categoryId: null, stockQuantity: 40, minStockLevel: 5, price: 100.00, status: 'Active', description: 'Traditional prasadam box', notes: '' }
    ];

    for (const product of products) {
      try {
        await axios.post(`${API_BASE}/products`, product);
        console.log(`✅ Created product: ${product.name}`);
      } catch (err) {
        console.log(`⚠️ Product ${product.name} might already exist`);
      }
    }

    // Create sample events
    console.log('Creating sample events...');
    
    // First get areas to use for events
    const areasResponse = await axios.get(`${API_BASE}/areas`);
    const areas = areasResponse.data;
    console.log(`Found ${areas.length} areas`);

    // Get event types
    const eventTypesResponse = await axios.get(`${API_BASE}/event-types`);
    const eventTypes = eventTypesResponse.data;
    console.log(`Found ${eventTypes.length} event types`);

    if (areas.length > 0 && eventTypes.length > 0) {
      const events = [
        {
          name: 'Daily Morning Pooja',
          areaId: areas[0].id,
          eventTypeId: eventTypes[0].id,
          startDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(), // Tomorrow
          endDate: new Date(Date.now() + 24 * 60 * 60 * 1000 + 2 * 60 * 60 * 1000).toISOString(), // Tomorrow + 2 hours
          description: 'Daily morning prayers and rituals',
          location: 'Main Temple',
          organizer: 'Temple Committee',
          contact: '9876543210',
          notes: 'All devotees welcome',
          status: 'Upcoming',
          isApprovalNeeded: false
        },
        {
          name: 'Devi Pooja',
          areaId: areas[0].id,
          eventTypeId: eventTypes.length > 1 ? eventTypes[1].id : eventTypes[0].id,
          startDate: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(), // Day after tomorrow
          endDate: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000 + 3 * 60 * 60 * 1000).toISOString(), // Day after tomorrow + 3 hours
          description: 'Special pooja for Devi',
          location: 'Sanctum Sanctorum',
          organizer: 'Head Priest',
          contact: '9876543211',
          notes: 'Special offerings required',
          status: 'Scheduled',
          isApprovalNeeded: false
        },
        {
          name: 'Festival Celebration',
          areaId: areas.length > 1 ? areas[1].id : areas[0].id,
          eventTypeId: eventTypes.length > 2 ? eventTypes[2].id : eventTypes[0].id,
          startDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // Next week
          endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000 + 6 * 60 * 60 * 1000).toISOString(), // Next week + 6 hours
          description: 'Annual festival celebration',
          location: 'Temple Grounds',
          organizer: 'Festival Committee',
          contact: '9876543212',
          notes: 'Grand celebration with cultural programs',
          status: 'Upcoming',
          isApprovalNeeded: true
        }
      ];

      for (const event of events) {
        try {
          await axios.post(`${API_BASE}/events`, event);
          console.log(`✅ Created event: ${event.name}`);
        } catch (err) {
          console.log(`⚠️ Event ${event.name} might already exist or failed: ${err.response?.data || err.message}`);
        }
      }
    }

    // Create sample sales
    console.log('Creating sample sales...');
    const sales = [
      {
        userId: 1,
        staffId: 1,
        saleDate: new Date().toISOString(),
        totalAmount: 125.00,
        discountAmount: 5.00,
        finalAmount: 120.00,
        paymentMethod: 'Cash',
        status: 'Completed',
        customerName: 'Devotee Ravi',
        customerPhone: '9876543213',
        staffName: 'Staff Member',
        notes: 'Regular customer',
        saleItems: []
      },
      {
        userId: 1,
        staffId: 1,
        saleDate: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(), // Yesterday
        totalAmount: 80.00,
        discountAmount: 0.00,
        finalAmount: 80.00,
        paymentMethod: 'UPI',
        status: 'Completed',
        customerName: 'Devotee Priya',
        customerPhone: '9876543214',
        staffName: 'Staff Member',
        notes: 'Festival purchase',
        saleItems: []
      },
      {
        userId: 1,
        staffId: 1,
        saleDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(), // Day before yesterday
        totalAmount: 200.00,
        discountAmount: 10.00,
        finalAmount: 190.00,
        paymentMethod: 'Card',
        status: 'Completed',
        customerName: 'Devotee Suresh',
        customerPhone: '9876543215',
        staffName: 'Staff Member',
        notes: 'Bulk purchase',
        saleItems: []
      }
    ];

    for (const sale of sales) {
      try {
        await axios.post(`${API_BASE}/sales`, sale);
        console.log(`✅ Created sale for: ${sale.customerName}`);
      } catch (err) {
        console.log(`⚠️ Sale for ${sale.customerName} failed: ${err.response?.data || err.message}`);
      }
    }

    console.log('🎉 Sample data seeding completed!');
    
    // Verify data was created
    const eventsCheck = await axios.get(`${API_BASE}/events`);
    const productsCheck = await axios.get(`${API_BASE}/products`);
    const salesCheck = await axios.get(`${API_BASE}/sales`);
    
    console.log(`📊 Final counts - Events: ${eventsCheck.data.length}, Products: ${productsCheck.data.length}, Sales: ${salesCheck.data.length}`);

  } catch (error) {
    console.error('❌ Error seeding data:', error.message);
    if (error.response) {
      console.error('Response:', error.response.data);
    }
  }
}

seedSampleData();
