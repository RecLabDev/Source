
pub fn mount(filter: &str) {
    // Setup a default tracing subscriber so we can see log output.
    // In a real environment, this would typically be setup during init
    //   with a more robust subscriber which can collect and re-route
    //   tracing events to a host-system.
    tracing_subscriber::fmt()
        .with_env_filter(filter)
        .with_thread_names(true)
        .with_thread_ids(false)
        .with_target(false)
        .with_file(true)
        .with_line_number(true)
        .with_timer(true)
        .without_time()
        .init();
}