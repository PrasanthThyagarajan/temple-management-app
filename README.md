# Devakaryam - Temple Management System

A comprehensive temple management system built with modern web technologies, designed to help temples manage devotees, donations, events, and temple information efficiently.

## 🏛️ Features

### Core Modules
- **Temple Management** - Manage multiple temples with detailed information
- **Devotee Management** - Track devotee details and registrations
- **Donation Management** - Handle donations with status tracking
- **Event Management** - Organize and manage temple events
- **Dashboard** - Comprehensive overview with statistics and recent activities

### Technical Features
- **Modern UI/UX** - Built with Vue 3 and Element Plus
- **Responsive Design** - Works seamlessly on all devices
- **Real-time Updates** - Live data updates and notifications
- **Secure API** - RESTful API with proper authentication
- **Database Integration** - Entity Framework with SQL Server

## 🚀 Technology Stack

### Frontend
- **Vue 3** - Progressive JavaScript framework
- **Element Plus** - Vue 3 UI component library
- **Vue Router 4** - Official router for Vue.js
- **Pinia** - State management for Vue
- **Axios** - HTTP client for API calls
- **Day.js** - Modern date manipulation library
- **Vite** - Fast build tool and dev server

### Backend
- **ASP.NET Core 8** - Cross-platform web framework
- **Entity Framework Core** - Object-relational mapping
- **SQL Server** - Relational database
- **C#** - Modern programming language

## 📁 Project Structure

```
temple-management-app/
├── temple-api/                 # Backend API (.NET Core)
│   ├── Controllers/           # API endpoints
│   ├── Models/               # Data models and DTOs
│   ├── Services/             # Business logic
│   ├── Data/                 # Database context
│   └── Tests/                # Unit tests
├── temple-ui/                 # Frontend application (Vue.js)
│   ├── src/
│   │   ├── views/            # Page components
│   │   ├── router/           # Vue router configuration
│   │   └── App.vue          # Main application component
│   ├── package.json          # Node.js dependencies
│   └── vite.config.js        # Vite configuration
└── README.md                  # Project documentation
```

## 🛠️ Installation & Setup

### Prerequisites
- **.NET 8 SDK** - For backend development
- **Node.js 18+** - For frontend development
- **SQL Server** - Database server
- **Git** - Version control

### Backend Setup
1. Navigate to the `temple-api` directory
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Update database connection string in `appsettings.json`
4. Run database migrations:
   ```bash
   dotnet ef database update
   ```
5. Start the API:
   ```bash
   dotnet run
   ```

### Frontend Setup
1. Navigate to the `temple-ui` directory
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start development server:
   ```bash
   npm run dev
   ```
4. Open http://localhost:3000 in your browser

## 🌐 API Endpoints

### Temples
- `GET /api/temples` - Get all temples
- `POST /api/temples` - Create new temple
- `PUT /api/temples/{id}` - Update temple
- `DELETE /api/temples/{id}` - Delete temple

### Devotees
- `GET /api/devotees` - Get all devotees
- `POST /api/devotees` - Register new devotee
- `PUT /api/devotees/{id}` - Update devotee
- `DELETE /api/devotees/{id}` - Remove devotee

### Donations
- `GET /api/donations` - Get all donations
- `POST /api/donations` - Record new donation
- `PUT /api/donations/{id}` - Update donation
- `DELETE /api/donations/{id}` - Remove donation

### Events
- `GET /api/events` - Get all events
- `POST /api/events` - Create new event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Cancel event

## 🎨 UI Components

The application uses Element Plus components for a consistent and professional look:

- **Navigation** - Horizontal menu with active state indicators
- **Cards** - Information display with headers and actions
- **Tables** - Data presentation with sorting and filtering
- **Forms** - Input validation and user-friendly interfaces
- **Icons** - Visual elements for better user experience

## 🔧 Development

### Building for Production
```bash
# Frontend
cd temple-ui
npm run build

# Backend
cd temple-api
dotnet publish -c Release
```

### Running Tests
```bash
# Backend tests
cd temple-api
dotnet test

# Frontend tests
cd temple-ui
npm run test:unit
```

## 📱 Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **Vue.js Team** - For the amazing frontend framework
- **Microsoft** - For .NET Core and Entity Framework
- **Element Plus** - For the beautiful UI components
- **Vite** - For the fast build tool

## 📞 Support

For support and questions, please open an issue on GitHub or contact the development team.

---

**Built with ❤️ for better temple management**
