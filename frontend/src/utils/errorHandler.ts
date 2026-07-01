// Global error handling utilities
export interface AppError {
  code: string
  message: string
  details?: any
  timestamp: string
}

export class ErrorHandler {
  private static errorMessages: Record<string, string> = {
    'NETWORK_ERROR': '網路連線失敗，請檢查您的網路設定',
    'UNAUTHORIZED': '請先登入',
    'FORBIDDEN': '您沒有權限執行此操作',
    'NOT_FOUND': '找不到請求的資源',
    'VALIDATION_ERROR': '請檢查輸入資料是否正確',
    'SERVER_ERROR': '伺服器錯誤，請稍後再試',
    'REQUEST_TIMEOUT': '請求超時，請檢查網路連線',
    'FILE_TOO_LARGE': '檔案大小超過限制',
    'FILE_TYPE_INVALID': '檔案格式不符',
    'TEMP_DATE_INVALID': '期間設定無效，結束日期不能早於開始日期',
    'SPOT_NOT_FOUND': '找不到該景點',
    'AUTH_ERROR': '認證失敗，請重新登入',
  }

  static getUserMessage(error: any): string {
    if (error.message) {
      return error.message
    }

    if (error.errorCode) {
      return this.errorMessages[error.errorCode] || error.errorCode
    }

    if (error.code) {
      return this.errorMessages[error.code] || error.code
    }

    return '發生錯誤，請稍後再試'
  }

  static getErrorAction(error: any): string {
    const errorCode = error.errorCode || error.code

    if (errorCode === 'NETWORK_ERROR') {
      return '請檢查您的網路連線'
    } else if (errorCode === 'UNAUTHORIZED' || errorCode === 'AUTH_ERROR') {
      return '請重新登入'
    } else if (errorCode === 'VALIDATION_ERROR') {
      return '請檢查輸入內容'
    } else if (errorCode === 'FILE_TOO_LARGE') {
      return '請選擇較小的檔案'
    } else if (errorCode === 'FILE_TYPE_INVALID') {
      return '請選擇正確的檔案格式'
    }

    return '請稍後再試'
  }

  static isAuthError(error: any): boolean {
    const errorCode = error.errorCode || error.code
    return errorCode === 'UNAUTHORIZED' || errorCode === 'AUTH_ERROR'
  }

  static isNetworkError(error: any): boolean {
    return error.errorCode === 'NETWORK_ERROR' || !error.response && !error.request
  }

  static isValidationError(error: any): boolean {
    return error.errorCode === 'VALIDATION_ERROR' || error.status === 400
  }

  static logError(error: any, context?: string) {
    const errorLog = {
      message: error.message || 'Unknown error',
      code: error.errorCode || error.code || 'UNKNOWN',
      context: context || 'No context',
      timestamp: new Date().toISOString(),
      stack: error.stack,
    }

    // In development, log to console
    if (import.meta.env.DEV) {
      console.error('Error logged:', errorLog)
    }

    // In production, you might want to send this to a logging service
    // Example: sendToLoggingService(errorLog)
  }

  static createError(
    code: string,
    message: string,
    details?: any
  ): AppError {
    return {
      code,
      message,
      details,
      timestamp: new Date().toISOString(),
    }
  }

  static sanitizeError(error: any): AppError {
    return {
      code: error.errorCode || error.code || 'UNKNOWN_ERROR',
      message: this.getUserMessage(error),
      details: error.details,
      timestamp: error.timestamp || new Date().toISOString(),
    }
  }
}

// Global error handler for unhandled errors
export const setupGlobalErrorHandling = () => {
  if (typeof window === 'undefined') return

  // Handle unhandled promise rejections
  window.addEventListener('unhandledrejection', (event) => {
    console.error('Unhandled promise rejection:', event.reason)
    ErrorHandler.logError(event.reason, 'Unhandled promise rejection')

    // Optionally show user-friendly message
    if (import.meta.env.PROD) {
      event.preventDefault()
    }
  })

  // Handle unhandled errors
  window.addEventListener('error', (event) => {
    console.error('Unhandled error:', event.error)
    ErrorHandler.logError(event.error, 'Unhandled error')

    // Optionally show user-friendly message
    if (import.meta.env.PROD) {
      event.preventDefault()
    }
  })
}

export default ErrorHandler