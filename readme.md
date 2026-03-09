# Capto

[中文文档](README_CN.md)

A lightweight, high-performance Windows screenshot tool with comprehensive annotation and editing capabilities.

![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

## ✨ Features

- 🚀 **Lightweight & High Performance** - Built on .NET 9.0 Windows Forms, low memory footprint, fast startup
- 🎨 **7 Drawing Tools** - Pen, Rectangle, Circle, Arrow, Blur, Text, Eraser
- 🖼️ **Region Capture** - Drag to select capture area, freely adjust size
- ⌨️ **Hotkey Support** - Default Alt+S, customizable hotkeys
- 📋 **One-Click Copy** - Copy to clipboard for quick sharing
- 💾 **PNG Save** - Auto-generated filenames, save locally
- 🎯 **High DPI Support** - Perfect support for high-resolution displays
- 🎨 **SVG Icons** - Rendered with SkiaSharp, crisp and beautiful
- 🔧 **Draggable Toolbar** - Flexible toolbar positioning

## 📸 Feature Showcase

### Screenshot Features
- Full-screen overlay, drag to select region
- Adjustable selection area
- Toolbar appears after selection

### Drawing Tools

| Tool | Function |
|------|----------|
| ✏️ Pen | Freehand drawing |
| ⬜ Rectangle | Draw rectangular frames |
| ⭕ Circle | Draw circles/ellipses |
| ➡️ Arrow | Draw arrow indicators |
| 🔲 Blur | Mosaic/blur effect |
| 📝 Text | Text input and annotation |
| 🧹 Eraser | Eraser effect |

### Tool Properties
- **Stroke Width**: 1px - 20px multiple pen sizes
- **Color Selection**: Red, Black, Blue, Green, Yellow and more preset colors
- **Arrow Style**: Selectable arrow styles

### Image Operations
- **Save**: PNG format, auto-generated filename
- **Copy**: One-click copy to clipboard
- **Cancel**: Close button only closes screenshot window

## 🛠️ Tech Stack

- **Framework**: .NET 9.0 Windows Forms
- **Graphics**: SkiaSharp (SVG icon rendering)
- **UI Components**: System.Windows.Forms
- **Language**: C#

## 📦 Installation

### Requirements
- Windows 10 or later
- .NET 9.0 Runtime

### Installation Steps
1. Download the latest Capto.exe
2. Run the application
3. Icon appears in system tray

## 📖 Usage

### Quick Start
1. Run the application, icon appears in system tray
2. Press **Alt+S** or click tray icon and select "Screenshot"
3. Drag to select capture area
4. Use toolbar for annotations
5. Click save or copy to complete

### Hotkeys

| Hotkey | Function |
|--------|----------|
| Alt+S | Screenshot |
| Esc | Cancel screenshot |

### System Tray
- **Double-click**: Open settings window
- **Right-click menu**:
  - Screenshot: Start capture
  - Settings: Open settings window
  - Exit: Exit application

### Settings
- Customizable screenshot hotkey
- Default hotkey: Alt+S

## 🏗️ Project Structure

```
Capto/
├── Program.cs                 # Application entry point
├── TrayIcon.cs               # System tray icon management
├── ScreenCaptureForm.cs      # Main screenshot window
├── ToolStripManager.cs       # Toolbar manager
├── SettingsForm.cs           # Settings window
├── TransparentTextBox.cs    # Transparent text box component
├── Base64Converter.cs        # Base64 conversion tool
├── DrawObjects/              # Drawing objects directory
│   ├── DrawObject.cs         # Drawing object base class
│   ├── PenDrawObject.cs      # Pen tool
│   ├── RectangleDrawObject.cs # Rectangle tool
│   ├── CircleDrawObject.cs   # Circle tool
│   ├── ArrowDrawObject.cs    # Arrow tool
│   ├── BlurDrawObject.cs     # Blur/mosaic tool
│   ├── TextDrawObject.cs     # Text tool
│   └── EraserDrawObject.cs   # Eraser tool
├── Utilities/
│   └── IconLoader.cs         # SVG icon loader
└── resource/                 # SVG icon resources
```

## 🔧 Development

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Publish
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 📋 Feature List

### Implemented Features ✅
- [x] Full-screen screenshot (Hotkey Alt+S)
- [x] Region screenshot (Drag selection)
- [x] Pen tool (Freehand drawing)
- [x] Rectangle tool (Draw rectangles)
- [x] Circle tool (Draw circles/ellipses)
- [x] Arrow tool (Draw arrows)
- [x] Blur tool (Mosaic effect)
- [x] Text tool (Text annotation)
- [x] Eraser tool (Eraser)
- [x] Pen size adjustment
- [x] Color selection
- [x] Copy to clipboard
- [x] Save to file (PNG)
- [x] System tray icon
- [x] Hotkey settings
- [x] Cancel save doesn't close window

### Planned Features ⏳
- [ ] Watermark feature
- [ ] Screen recording
- [ ] Long screenshot
- [ ] Pin screenshot
- [ ] More image formats (JPG, BMP)
- [ ] Font style selection (Text tool)

## 🎯 Project Highlights

### 1. Lightweight & High Performance
- Built on .NET 9.0 Windows Forms, low memory usage
- Single executable file, fast startup
- Graphics-based drawing, efficient performance

### 2. High DPI Support
- Automatic DPI awareness on startup
- Crystal clear on high-resolution displays

### 3. Complete Drawing Toolkit
Provides 7 professional drawing tools: Pen, Rectangle, Circle, Arrow, Blur, Text, Eraser

### 4. Modern UI Design
- SVG icon rendering (SkiaSharp), crisp and beautiful
- Semi-transparent toolbar
- Draggable toolbar position
- One-click color and size switching

### 5. Low-Coupling Architecture
- Event-driven design: Toolbar communicates with main window via events
- Object-oriented: Drawing object inheritance system, easy to extend

### 6. High System Integration
- System tray resident
- Alt+S hotkey support
- Clipboard copy support

### 7. Strong Customization
- Customizable screenshot hotkey
- Draggable toolbar, adapts to different scenarios

## 📊 Technical Comparison

| Feature | Capto | Common Screenshot Tools |
|---------|-------|-------------------------|
| Size | ~Few MB | Tens of MB |
| Dependencies | .NET Runtime | Many dependencies |
| Extensibility | Object-oriented | Fixed features |
| High DPI | Native support | Extra adaptation needed |

## 📄 License

BSD 3-Clause License

## 🤝 Contributing

Issues and Pull Requests are welcome!

## 📮 Contact

For questions or suggestions, feel free to reach out via Issues or contact me at: 5439874@qq.com
