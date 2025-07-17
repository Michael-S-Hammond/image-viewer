//
//  ImageViewerViewModel.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import Foundation
import SwiftUI

@MainActor
class ImageViewerViewModel: ObservableObject {
    @Published var images: [ImageInfo] = []
    @Published var displayedImages: [ImageInfo] = []
    @Published var currentImageIndex: Int = -1
    @Published var currentFolderPath: String = ""
    @Published var isLoopEnabled: Bool = true
    @Published var currentSortOption: SortOption = .name
    @Published var currentTheme: ThemeOption = .dark
    @Published var showingFolderPicker = false
    @Published var errorMessage: String?
    @Published var showingError = false
    @Published var isLoadingMore = false
    
    private let imageService = ImageService()
    private let batchSize = 100
    private var currentBatch = 0
    
    var currentImage: ImageInfo? {
        guard currentImageIndex >= 0 && currentImageIndex < images.count else { return nil }
        return images[currentImageIndex]
    }
    
    var windowTitle: String {
        if let currentImage = currentImage {
            return "Image Viewer - \(currentImage.fileName)"
        }
        return "Image Viewer"
    }
    
    var displayedCurrentIndex: Int {
        currentImageIndex + 1
    }
    
    var totalImages: Int {
        images.count
    }
    
    var hasImages: Bool {
        !images.isEmpty
    }
    
    var canNavigatePrevious: Bool {
        hasImages
    }
    
    var canNavigateNext: Bool {
        hasImages
    }
    
    func loadImagesFromFolder(_ folderURL: URL) {
        do {
            let loadedImages = try imageService.loadImagesFromFolder(folderURL)
            
            self.images = sortImages(loadedImages, by: currentSortOption)
            self.currentFolderPath = folderURL.path
            self.currentBatch = 0
            
            loadInitialBatch()
            
            if !self.images.isEmpty {
                self.currentImageIndex = 0
            } else {
                self.currentImageIndex = -1
            }
        } catch {
            self.errorMessage = error.localizedDescription
            self.showingError = true
        }
    }
    
    private func loadInitialBatch() {
        let endIndex = min(batchSize, images.count)
        displayedImages = Array(images[0..<endIndex])
        currentBatch = 0
    }
    
    func loadMoreImagesIfNeeded(currentIndex: Int) {
        let threshold = displayedImages.count - 10
        
        guard currentIndex >= threshold,
              !isLoadingMore,
              displayedImages.count < images.count else { return }
        
        loadNextBatch()
    }
    
    private func loadNextBatch() {
        isLoadingMore = true
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) { [weak self] in
            guard let self = self else { return }
            
            let startIndex = self.displayedImages.count
            let endIndex = min(startIndex + self.batchSize, self.images.count)
            
            if startIndex < endIndex {
                let newImages = Array(self.images[startIndex..<endIndex])
                self.displayedImages.append(contentsOf: newImages)
                self.currentBatch += 1
            }
            
            self.isLoadingMore = false
        }
    }
    
    func navigatePrevious() {
        guard hasImages else { return }
        
        if currentImageIndex > 0 {
            currentImageIndex -= 1
        } else {
            currentImageIndex = images.count - 1
        }
    }
    
    func navigateNext() {
        guard hasImages else { return }
        
        if currentImageIndex < images.count - 1 {
            currentImageIndex += 1
        } else {
            currentImageIndex = 0
        }
    }
    
    func reverseOrder() {
        guard images.count > 1 else { return }
        
        let currentFileName = currentImage?.fileName
        images.reverse()
        
        loadInitialBatch()
        
        if let fileName = currentFileName,
           let newIndex = images.firstIndex(where: { $0.fileName == fileName }) {
            currentImageIndex = newIndex
        } else {
            currentImageIndex = 0
        }
    }
    
    func updateSortOption(_ sortOption: SortOption) {
        guard currentSortOption != sortOption else { return }
        
        let currentFileName = currentImage?.fileName
        currentSortOption = sortOption
        images = sortImages(images, by: sortOption)
        
        loadInitialBatch()
        
        if let fileName = currentFileName,
           let newIndex = images.firstIndex(where: { $0.fileName == fileName }) {
            currentImageIndex = newIndex
        } else if !images.isEmpty {
            currentImageIndex = 0
        }
    }
    
    private func sortImages(_ images: [ImageInfo], by sortOption: SortOption) -> [ImageInfo] {
        switch sortOption {
        case .name:
            return images.sorted { $0.fileName.localizedCaseInsensitiveCompare($1.fileName) == .orderedAscending }
        case .date:
            return images.sorted { $0.dateModified < $1.dateModified }
        case .random:
            return images.shuffled()
        }
    }
}