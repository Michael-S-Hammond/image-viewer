//
//  ImageViewer_macApp.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import SwiftUI

@main
struct ImageViewer_macApp: App {
    var body: some Scene {
        Window("Image Viewer", id: "image-viewer") {
            ContentView()
        }
        .commands {
            CommandGroup(replacing: .newItem) {
                EmptyView()
            }

//            CommandMenu("File") {
            CommandGroup(after: .newItem) {
                Button("Open Folder...") {
                    NotificationCenter.default.post(name: .openFolder, object: nil)
                }
                .keyboardShortcut("o", modifiers: .command)
            }
            
            CommandGroup(before: .toolbar) {
                Button("Previous Image") {
                    NotificationCenter.default.post(name: .navigatePrevious, object: nil)
                }
                .keyboardShortcut(.leftArrow, modifiers: [])
                
                Button("Next Image") {
                    NotificationCenter.default.post(name: .navigateNext, object: nil)
                }
                .keyboardShortcut(.rightArrow, modifiers: [])
                
                Divider()
                
                Button("Sort by Name") {
                    NotificationCenter.default.post(name: .sortByName, object: nil)
                }
                .keyboardShortcut("1", modifiers: .command)
                
                Button("Sort by Date") {
                    NotificationCenter.default.post(name: .sortByDate, object: nil)
                }
                .keyboardShortcut("2", modifiers: .command)
                
                Button("Sort by Random") {
                    NotificationCenter.default.post(name: .sortByRandom, object: nil)
                }
                .keyboardShortcut("3", modifiers: .command)
                
                Divider()
                
                Button("Reverse Order") {
                    NotificationCenter.default.post(name: .reverseOrder, object: nil)
                }
                
                Divider()
                
                Button("Dark Theme") {
                    NotificationCenter.default.post(name: .setDarkTheme, object: nil)
                }
                
                Button("Light Theme") {
                    NotificationCenter.default.post(name: .setLightTheme, object: nil)
                }
                
                Divider()
            }
        }
    }
}

// Notification names for menu commands
extension Notification.Name {
    static let openFolder = Notification.Name("openFolder")
    static let navigatePrevious = Notification.Name("navigatePrevious")
    static let navigateNext = Notification.Name("navigateNext")
    static let sortByName = Notification.Name("sortByName")
    static let sortByDate = Notification.Name("sortByDate")
    static let sortByRandom = Notification.Name("sortByRandom")
    static let reverseOrder = Notification.Name("reverseOrder")
    static let setDarkTheme = Notification.Name("setDarkTheme")
    static let setLightTheme = Notification.Name("setLightTheme")
}
