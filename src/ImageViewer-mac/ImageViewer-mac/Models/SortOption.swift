//
//  SortOption.swift
//  ImageViewer-mac
//
//  Created by Michael Hammond on 7/16/25.
//

import Foundation

enum SortOption: String, CaseIterable, Identifiable {
    case name = "By Name"
    case date = "By Date"
    case random = "By Random"
    
    var id: String { rawValue }
}