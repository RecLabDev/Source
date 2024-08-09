#![allow(unreachable_code)]

use std::process::ExitCode;
use std::time::Duration;

use tokio::time::sleep;

use anyhow::Result;

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<ExitCode> {
    println!("Before Spawn ..");
    
    let _hello = tokio::spawn(async move {
        println!("Before Loop ..");
        loop {
            //..
            sleep(Duration::from_secs(1)).await;
            println!("After Sleep ..");
        }
        println!("After Loop ..");
    });
    
    println!("After Spawn ..");
    
    // hello.await?;
    sleep(Duration::from_secs(5)).await;
    
    println!("After Await ..");
    
    Ok(ExitCode::SUCCESS)
}