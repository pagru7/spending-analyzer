# Spending Analyzer Mobile

A mobile application built with .NET for Android to help users track and analyze their spending habits.
It is a mobile client for Spending Analyzer web application.

## Features

- **Dashboard**: View summary of spending.
- **Add Transaction**: Easily record new expenses or income.
- **Transaction History**: View a list of past transactions.
- **Settings**: Configure application preferences.
- **Data Persistence**: Uses a local database for storing transaction data.

## Getting Started

### Prerequisites

- Visual Studio or Visual Studio Code with .NET MAUI / Android workloads installed.
- .NET 8.0 SDK (or later).
- Android Emulator or a physical Android device for testing.

### Building and Running

1. **Clone the repository:**

   ```bash
   git clone <repository-url>
   ```

2. **Open the project:**
   Open the solution folder in your preferred IDE.

3. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

4. **Run the application:**
   Select your target device (Emulator or Physical Device) and run the application.

## Project Structure

- `src/Spending Analyzer Mobile/`: Main application source code.
  - `Activities/`: Android Activities (screens).
  - `Adapters/`: List adapters for UI components.
  - `Models/`: Data models.
  - `Services/`: Business logic and data access services.
  - `Resources/`: UI layouts, strings, and images.
