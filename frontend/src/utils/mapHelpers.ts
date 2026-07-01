// Global utilities for map interactions
declare global {
  interface Window {
    viewSpotDetails: (spotId: number) => void
  }
}

export const setupMapHelpers = () => {
  // This will be set by the main app component
  window.viewSpotDetails = (spotId: number) => {
    console.log('View spot details:', spotId)
    // The actual implementation will be provided by the Vue app
  }
}

// Call this during app initialization
if (typeof window !== 'undefined') {
  setupMapHelpers()
}