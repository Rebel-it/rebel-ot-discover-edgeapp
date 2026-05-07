export interface ApiResponseObject<T> {
    success: boolean
    errorMessage?: string;
    data?: T
}