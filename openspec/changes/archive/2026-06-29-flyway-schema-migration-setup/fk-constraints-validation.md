# Foreign Key Constraints 驗證報告

## FK Constraints 詳細檢查

### V3__create_refresh_tokens.sql
| FK Constraint | References | Target Version | Check |
|---------------|------------|----------------|-------|
| fk_refresh_tokens_user | users (id) | V2 | ✅ V3 > V2 |

### V4__create_spots.sql  
| FK Constraint | References | Target Version | Check |
|---------------|------------|----------------|-------|
| fk_spots_submitted_by | users (id) | V2 | ✅ V4 > V2 |
| fk_spots_reviewed_by | users (id) | V2 | ✅ V4 > V2 |

### V5__create_spot_images.sql
| FK Constraint | References | Target Version | Check |
|---------------|------------|----------------|-------|
| fk_spot_images_spot | spots (id) | V4 | ✅ V5 > V4 |
| fk_spot_images_uploaded_by | users (id) | V2 | ✅ V5 > V2 |

### V6__create_reviews.sql
| FK Constraint | References | Target Version | Check |
|---------------|------------|----------------|-------|
| fk_reviews_spot | spots (id) | V4 | ✅ V6 > V4 |
| fk_reviews_user | users (id) | V2 | ✅ V6 > V2 |

### V7__create_import_history.sql
| FK Constraint | References | Target Version | Check |
|---------------|------------|----------------|-------|
| fk_import_history_operated_by | users (id) | V2 | ✅ V7 > V2 |

## 刪除策略分析

### CASCADE DELETE (強刪除)
- **refresh_tokens.user_id** → 當 user 刪除時，相關 tokens 自動刪除
- **spot_images.spot_id** → 當 spot 刪除時，相關圖片自動刪除  
- **reviews.user_id** → 當 user 刪除時，相關評價自動刪除
- **reviews.spot_id** → 當 spot 刪除時，相關評價自動刪除

### SET NULL (弱關聯)
- **spots.submitted_by** → 當 user 刪除時，submitted_by 設為 NULL
- **spots.reviewed_by** → 當 user 刪除時，reviewed_by 設為 NULL
- **spot_images.uploaded_by** → 當 user 刪除時，uploaded_by 設為 NULL
- **import_history.operated_by** → 當 user 刪除時，operated_by 設為 NULL

## 驗證結果

✅ **所有 8 個 FK constraints 都正確引用先前版本建立的資料表**
✅ **無向前引用 (forward references) 問題**
✅ **刪除策略設計合理：CASCADE 用於強關聯，SET NULL 用於可選關聯**
✅ **符合軟刪除原則：spots 採 deleted_at，關聯資料不因軟刪除而消失**