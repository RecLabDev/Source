# Aby

Aby is an application development framework focused on making the creation of distributed, networked applications easier and faster across multiple platforms.

## 1. Summary

### SDK, Plugins, & Utils

Aby targets both C# and TypeScript to allow rapid development of networked administrative tools for Unity-based projects.

In the future, Aby aims to provide additional plugins for other environments (Unreal, etc.) via plugins for their respective engines.

- **Core Utilities:** WebService providing HTTP endpoints and a WebSocket for state updates, bridge of WebService for Unity (via Plugin as a ScriptableObject), data serialization, and other fundamental tasks.
- **Tests & Examples:** Implementation Sandbox with a Web interface, an Editor UI, a 2D sprite representation, and data from the shared state.

### Dashboard

The Dashboard is a TypeScript project which constructs in-editor UI using the SDK + Unity plugin. It also provides a basic implementation for a REST web-service that could be used to serve files over HTTP.

- **Monitoring:** View real-time metrics for the embedded service, including active connections and server health.
- **Analytics:** Analyze usage patterns, performance metrics, and other valuable analytical data.
- **Configuration Management:** User-friendly interface to manage configurations for the SDK and Unity plugin.
- **User Management:** Handle user roles, access controls, and view audit logs.

## 2. Architecture

Aby's architecture is crafted to be modular, scalable, and maintainable, guaranteeing a seamless development and operational journey.

### Tools

- **.NET SDK** Development tools for building C#-based libs+tools.
  - **NuGet** Primary package-manager for C# projects.
- **Node.js, Yarn, and Vite:** Supply Dev tooling with hot-reload.
  - **React:** UI, Routing, and a REST API for the admin panel and other web-based interfaces.
  - **Tailwind and DaisyUI:** Utilized for site-wide styling and layout to deliver a consistent and modern user interface.
- **MarkdownBook:** Utilized for documentation tooling to ensure well-documented code and features, aiding in maintenance and user assistance.
- **Unity:** Secondary support target.

### Data Storage

- **PocketBase:** Selected to offer auth and real-time data storage for the platform, ensuring swift and reliable data access and synchronization across different components.

### Ops & Infrastructure

- **Docker:** containerizing services within the Platform, ensuring a consistent and isolated environment for each service.
  - **Docker Compose:** Will be utilized both in dev and deployment, simplifying the management and orchestration of multi-container Docker applications.

The architectural design ensures each component can be developed, tested, and deployed independently, reducing dependencies and speeding up the development lifecycle. Each tool and technology was meticulously chosen for its capability to contribute to a robust, scalable, and maintainable system, aligning with the project’s long-term objectives and the anticipated needs of the user base.

## 2. Development Plan

### SDK Development

- [x] Develop core utilities for network communication.
- [ ] Establish basic logging facilities.

- [ ] Develop integration libraries for Unity.
- [ ] Create basic testing utilities.

- [ ] Perform integration testing and bug fixes.

### Plugin Development

- [ ] Develop Sandbox scenes and Editor panels for Unity.
- [ ] Implement Game Server manager for both engines.

- [ ] Develop monitoring and debugging utilities for Unity plugins.
- [ ] Implement real-time metrics display in editor panels.

- [ ] Perform integration testing and bug fixes.

### Dashboard Development

- [ ] Develop the basic structure of the Dashboard.
- [ ] Implement real-time monitoring and analytics.

- [ ] Develop configuration and user management interfaces.
- [ ] Integrate SDK, Unity, and plugins with the Dashboard.

- [ ] Perform integration testing and bug fixes.

### Admin Panel Development

- [ ] Setup React, Tailwind, and DaisyUI for Admin panel development.
- [ ] Develop basic UI and routing.

- [ ] Implement REST API for Admin panel.
- [ ] Integrate Admin panel with Dashboard and other components.

- [ ] Perform integration testing and bug fixes.

- [ ] Conduct final integration testing and bug fixes.

### Deployment and Monitoring

- [ ] Setup Docker and Docker Compose for deployment.
- [ ] Deploy the framework, monitor performance, and fix emergent issues.

### Docs and Final Testing

- [ ] Complete documentation using MarkdownBook.
