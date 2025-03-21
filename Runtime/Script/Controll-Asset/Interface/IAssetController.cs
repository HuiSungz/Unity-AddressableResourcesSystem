
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace AddressableManage
{
    public interface IAssetController
    {
        /// <summary>
        /// 배치 로드를 수행합니다.
        /// </summary>
        /// <param name="labelReferences">배치 로드할 항목</param>
        /// <returns>비동기 로드 결과</returns>
        ARMOperationHandle<bool> BatchLoad(List<AssetLabelReference> labelReferences);
        
        /// <summary>
        /// 개별 에셋을 문자열 키로 로드합니다.
        /// </summary>
        /// <param name="key">로드할 에셋의 키</param>
        /// <returns>비동기 로드 결과</returns>
        ARMOperationHandle<AssetEntry> Load(string key);
        
        /// <summary>
        /// 개별 에셋을 AssetReference로 로드합니다.
        /// </summary>
        /// <param name="assetReference">로드할 에셋 참조</param>
        /// <returns>비동기 로드 결과</returns>
        ARMOperationHandle<AssetEntry> Load(AssetReference assetReference);
        
        /// <summary>
        /// 지정된 라벨 참조에 해당하는 배치 로드된 에셋들을 해제합니다.
        /// </summary>
        /// <param name="labelReferences">해제할 에셋의 라벨 참조 목록</param>
        void ReleaseBatches(List<AssetLabelReference> labelReferences);
        
        /// <summary>
        /// 모든 배치 로드된 에셋을 해제합니다.
        /// </summary>
        void ReleaseAllBatches();
        
        /// <summary>
        /// 이미 로드된 에셋을 동기적으로 가져옵니다. 로드되지 않은 에셋은 로드하지 않습니다.
        /// </summary>
        /// <param name="key">가져올 에셋의 키</param>
        /// <param name="assetEntry">찾은 에셋 (찾지 못한 경우 null)</param>
        /// <returns>에셋을 찾았으면 true, 그렇지 않으면 false</returns>
        bool TryGetLoadedEntry(string key, out AssetEntry assetEntry);
        
        /// <summary>
        /// 이미 로드된 에셋을 동기적으로 가져옵니다. 로드되지 않은 에셋은 로드하지 않습니다.
        /// </summary>
        /// <param name="assetReference">가져올 에셋 참조</param>
        /// <param name="assetEntry">찾은 에셋 (찾지 못한 경우 null)</param>
        /// <returns>에셋을 찾았으면 true, 그렇지 않으면 false</returns>
        bool TryGetLoadedEntry(AssetReference assetReference, out AssetEntry assetEntry);
    }
}