//
//  ContentView.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import SwiftUI
import UniformTypeIdentifiers

struct ContentView: View {
    @StateObject private var viewModel = ImageViewerViewModel()
    @Environment(\.openWindow) private var openWindow
    
    var body: some View {
        HSplitView {
            // Sidebar
            VStack(spacing: 0) {
                // Toolbar
                HStack {
                    Menu {
                        ForEach(SortOption.allCases) { option in
                            Button(option.rawValue) {
                                viewModel.updateSortOption(option)
                            }
                        }
                    } label: {
                        Label("Sort", systemImage: "arrow.up.arrow.down")
                    }
                    
                    Spacer()
                }
                .padding()
                .background(Color(NSColor.controlBackgroundColor))
                
                // Image List
                if viewModel.hasImages {
                    List(viewModel.images.indices, id: \.self, selection: Binding(
                        get: { viewModel.currentImageIndex >= 0 ? viewModel.currentImageIndex : nil },
                        set: { viewModel.currentImageIndex = $0 ?? 0 }
                    )) { index in
                        VStack(alignment: .leading, spacing: 4) {
                            Text(viewModel.images[index].fileName)
                                .font(.headline)
                                .lineLimit(2)
                            Text(viewModel.images[index].fileSize)
                                .font(.caption)
                                .foregroundColor(.secondary)
                        }
                        .padding(.vertical, 2)
                        .tag(index)
                    }
                    .listStyle(SidebarListStyle())
                } else {
                    VStack {
                        Spacer()
                        Text("No images loaded")
                            .font(.title2)
                            .foregroundColor(.secondary)
                        Text("Use ⌘O to open a folder")
                            .font(.caption)
                            .foregroundColor(.secondary)
                        Spacer()
                    }
                }
            }
            .frame(minWidth: 200)
            
            // Main Content Area
            VStack(spacing: 0) {
                // Image Display
                if let currentImage = viewModel.currentImage {
                    MediaDisplayView(
                        imageInfo: currentImage,
                        isLoopEnabled: viewModel.isLoopEnabled
                    )
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
                } else {
                    VStack {
                        Spacer()
                        Image(systemName: "photo")
                            .font(.system(size: 64))
                            .foregroundColor(.secondary)
                        Text("Select an image to view")
                            .font(.title2)
                            .foregroundColor(.secondary)
                        Spacer()
                    }
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
                }
                
                // Bottom Controls
                if viewModel.hasImages {
                    VStack(spacing: 12) {
                        // Navigation Controls
                        HStack {
                            Button(action: viewModel.navigatePrevious) {
                                Label("Previous", systemImage: "chevron.left")
                            }
                            .keyboardShortcut(.leftArrow, modifiers: [])
                            .disabled(!viewModel.canNavigatePrevious)
                            
                            Spacer()
                            
                            Text("\(viewModel.displayedCurrentIndex) of \(viewModel.totalImages)")
                                .font(.headline)
                            
                            Spacer()
                            
                            Button(action: viewModel.navigateNext) {
                                Label("Next", systemImage: "chevron.right")
                            }
                            .keyboardShortcut(.rightArrow, modifiers: [])
                            .disabled(!viewModel.canNavigateNext)
                        }
                        
                        // Additional Controls
                        HStack {
                            Button(action: viewModel.reverseOrder) {
                                Label("Reverse", systemImage: "arrow.up.arrow.down")
                            }
                            .disabled(!viewModel.hasImages)
                            
                            Spacer()
                            
                            if viewModel.currentImage?.hasLoopableContent == true {
                                Toggle("Loop", isOn: $viewModel.isLoopEnabled)
                                    .toggleStyle(CheckboxToggleStyle())
                            }
                        }
                        
                        // Current Folder Path
                        if !viewModel.currentFolderPath.isEmpty {
                            Text(viewModel.currentFolderPath)
                                .font(.caption)
                                .foregroundColor(.secondary)
                                .lineLimit(1)
                                .truncationMode(.middle)
                        }
                    }
                    .padding()
                    .background(Color(NSColor.controlBackgroundColor))
                }
            }
            .frame(minWidth: 400)
        }
        .navigationTitle(viewModel.windowTitle)
        .fileImporter(
            isPresented: $viewModel.showingFolderPicker,
            allowedContentTypes: [.folder],
            allowsMultipleSelection: false
        ) { result in
            switch result {
            case .success(let urls):
                if let url = urls.first {
                    viewModel.loadImagesFromFolder(url)
                }
            case .failure(let error):
                viewModel.errorMessage = error.localizedDescription
                viewModel.showingError = true
            }
        }
        .alert("Error", isPresented: $viewModel.showingError) {
            Button("OK") { }
        } message: {
            Text(viewModel.errorMessage ?? "An unknown error occurred")
        }
        .preferredColorScheme(colorScheme)
        .onReceive(NotificationCenter.default.publisher(for: .openFolder)) { _ in
            viewModel.showingFolderPicker = true
        }
        .onReceive(NotificationCenter.default.publisher(for: .navigatePrevious)) { _ in
            viewModel.navigatePrevious()
        }
        .onReceive(NotificationCenter.default.publisher(for: .navigateNext)) { _ in
            viewModel.navigateNext()
        }
        .onReceive(NotificationCenter.default.publisher(for: .sortByName)) { _ in
            viewModel.updateSortOption(.name)
        }
        .onReceive(NotificationCenter.default.publisher(for: .sortByDate)) { _ in
            viewModel.updateSortOption(.date)
        }
        .onReceive(NotificationCenter.default.publisher(for: .sortByRandom)) { _ in
            viewModel.updateSortOption(.random)
        }
        .onReceive(NotificationCenter.default.publisher(for: .reverseOrder)) { _ in
            viewModel.reverseOrder()
        }
        .onReceive(NotificationCenter.default.publisher(for: .setDarkTheme)) { _ in
            viewModel.currentTheme = .dark
        }
        .onReceive(NotificationCenter.default.publisher(for: .setLightTheme)) { _ in
            viewModel.currentTheme = .light
        }
    }
    
    private var colorScheme: ColorScheme? {
        switch viewModel.currentTheme {
        case .dark:
            return .dark
        case .light:
            return .light
        }
    }
}

#Preview {
    ContentView()
        .frame(width: 1000, height: 700)
}
