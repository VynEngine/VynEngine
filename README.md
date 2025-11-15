<p align="center">
  <img src=".assets/banner.png" alt="VynEngine Banner" />
</p>

# VynEngine

**The modern Visual Novel engine with focus on ease-of-use.**  
Built with **.NET 9**, featuring a cross-platform editor powered by **Vue 3** and **Photino.NET**.

---

⚠️ **Disclaimer**  
VynEngine is an early-stage hobby project.  
It is **not production-ready** and subject to rapid changes.

---

## ✨ Overview

VynEngine aims to make the creation of visual novels and story-driven experiences as smooth as possible.  
The editor provides a node-based flow designer, live preview, and a modern UI inspired by professional tools.

---

## 🛠 Tech Stack

- **Runtime / Engine Core:**
    - C# / .NET 9
    - Silk.NET (OpenGL graphics, input, audio)
- **Editor Frontend:**
    - Vue 3 + TypeScript
    - SCSS
- **Editor Host:**
    - Photino.NET (Chromeless WebView host)
- **Tooling:**
    - Node.js ≥ 22.12 (recommended)
    - Vite (dev server & bundler)

---

## 📦 Requirements

To build and run VynEngine locally you’ll need:

- [.NET 9 SDK](https://dotnet.microsoft.com/)
- [Node.js 22.12+](https://nodejs.org/) (for building the Vue frontend)
- Git (to clone the repository)

---

## 🚀 Getting Started

```bash
# clone the repository
git clone https://github.com/VynEngine/VynEngine
cd VynEngine

# install frontend deps
cd VynEngine.Editor
cd UserInterface
npm install

# run the frontend (dev mode, hot reload)
npm run dev

# in another terminal, build & run the Photino.NET host
cd ..
cd ..
dotnet run --project VynEngine.Editor
```

The editor will load the Vue frontend from the dev server in development.
For production builds, the Vue app is bundled and served via Photino’s static file server.

---

## 💡 Contributing & Feedback

This is a hobby project, but contributions are welcome!
Ways you can help:
- Report bugs and issues
- Suggest features or improvements
- Contribute code (new nodes, editor features, engine runtime)
- Share feedback on usability and workflow

Please open an issue or discussion before starting larger contributions so we can coordinate.

📜 License

VynEngine is licensed under the [Apache License 2.0](LICENSE).