using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static BattleController;

public class CellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2Int coord;

    [Header("UI")]
    [SerializeField] private Image img;

    [Header("Base Sprites")]
    [SerializeField] private Sprite seaSprite;
    [SerializeField] private Sprite gunMissSprite;
    [SerializeField] private Sprite gunHitSprite;

    [SerializeField] private Sprite scoutShipSprite;
    [SerializeField] private Sprite scoutDamagedShipSprite;
    [SerializeField] private Sprite scoutEmptySprite;

    [Header("Bomb Miss Sprites")]
    [SerializeField] private Sprite bombMissTL;
    [SerializeField] private Sprite bombMissTR;
    [SerializeField] private Sprite bombMissBL;
    [SerializeField] private Sprite bombMissBR;

    [Header("Bomb Hit Sprites")]
    [SerializeField] private Sprite bombHitTL;
    [SerializeField] private Sprite bombHitTR;
    [SerializeField] private Sprite bombHitBL;
    [SerializeField] private Sprite bombHitBR;

    [Header("Torpedo Overlay")]
    [SerializeField] private Image torpedoOverlayImage;

    [Header("Torpedo Right Miss Sprites")]
    [SerializeField] private Sprite torpedoRightMiss0;
    [SerializeField] private Sprite torpedoRightMiss1;
    [SerializeField] private Sprite torpedoRightMiss2;
    [SerializeField] private Sprite torpedoRightMiss3;
    [SerializeField] private Sprite torpedoRightMiss4;
    [Header("Torpedo Right Hit Sprites")]
    [SerializeField] private Sprite torpedoRightHit0;
    [SerializeField] private Sprite torpedoRightHit1;
    [SerializeField] private Sprite torpedoRightHit2;
    [SerializeField] private Sprite torpedoRightHit3;
    [SerializeField] private Sprite torpedoRightHit4;

    [Header("Torpedo Left Miss Sprites")]
    [SerializeField] private Sprite torpedoLeftMiss0;
    [SerializeField] private Sprite torpedoLeftMiss1;
    [SerializeField] private Sprite torpedoLeftMiss2;
    [SerializeField] private Sprite torpedoLeftMiss3;
    [SerializeField] private Sprite torpedoLeftMiss4;
    [Header("Torpedo Left Hit Sprites")]
    [SerializeField] private Sprite torpedoLeftHit0;
    [SerializeField] private Sprite torpedoLeftHit1;
    [SerializeField] private Sprite torpedoLeftHit2;
    [SerializeField] private Sprite torpedoLeftHit3;
    [SerializeField] private Sprite torpedoLeftHit4;

    [Header("Torpedo Up Miss Sprites")]
    [SerializeField] private Sprite torpedoUpMiss0;
    [SerializeField] private Sprite torpedoUpMiss1;
    [SerializeField] private Sprite torpedoUpMiss2;
    [SerializeField] private Sprite torpedoUpMiss3;
    [SerializeField] private Sprite torpedoUpMiss4;
    [Header("Torpedo Up Hit Sprites")]
    [SerializeField] private Sprite torpedoUpHit0;
    [SerializeField] private Sprite torpedoUpHit1;
    [SerializeField] private Sprite torpedoUpHit2;
    [SerializeField] private Sprite torpedoUpHit3;
    [SerializeField] private Sprite torpedoUpHit4;

    [Header("Torpedo Down Miss Sprites")]
    [SerializeField] private Sprite torpedoDownMiss0;
    [SerializeField] private Sprite torpedoDownMiss1;
    [SerializeField] private Sprite torpedoDownMiss2;
    [SerializeField] private Sprite torpedoDownMiss3;
    [SerializeField] private Sprite torpedoDownMiss4;
    [Header("Torpedo Down Hit Sprites")]
    [SerializeField] private Sprite torpedoDownHit0;
    [SerializeField] private Sprite torpedoDownHit1;
    [SerializeField] private Sprite torpedoDownHit2;
    [SerializeField] private Sprite torpedoDownHit3;
    [SerializeField] private Sprite torpedoDownHit4;

    [SerializeField] private UnityEngine.UI.Image previewOverlayImage;
    [SerializeField] private UnityEngine.UI.Image bombOverlayImage;

    private Sprite[] torpedoRightMissSprites;
    private Sprite[] torpedoRightHitSprites;

    private Sprite[] torpedoLeftMissSprites;
    private Sprite[] torpedoLeftHitSprites;

    private Sprite[] torpedoUpMissSprites;
    private Sprite[] torpedoUpHitSprites;

    private Sprite[] torpedoDownMissSprites;
    private Sprite[] torpedoDownHitSprites;

    private Action<Vector2Int> onClick;
    private Action<Vector2Int> onHoverEnter;
    private Action<Vector2Int> onHoverExit;

    void Awake()
    {
        torpedoRightMissSprites = new Sprite[5]
        {
        torpedoRightMiss0,
        torpedoRightMiss1,
        torpedoRightMiss2,
        torpedoRightMiss3,
        torpedoRightMiss4
        };

        torpedoRightHitSprites = new Sprite[5]
        {
        torpedoRightHit0,
        torpedoRightHit1,
        torpedoRightHit2,
        torpedoRightHit3,
        torpedoRightHit4
        };

        torpedoLeftMissSprites = new Sprite[5]
        {
        torpedoLeftMiss0,
        torpedoLeftMiss1,
        torpedoLeftMiss2,
        torpedoLeftMiss3,
        torpedoLeftMiss4
        };

        torpedoLeftHitSprites = new Sprite[5]
        {
        torpedoLeftHit0,
        torpedoLeftHit1,
        torpedoLeftHit2,
        torpedoLeftHit3,
        torpedoLeftHit4
        };

        torpedoUpMissSprites = new Sprite[5]
        {
        torpedoUpMiss0,
        torpedoUpMiss1,
        torpedoUpMiss2,
        torpedoUpMiss3,
        torpedoUpMiss4
        };

        torpedoUpHitSprites = new Sprite[5]
        {
        torpedoUpHit0,
        torpedoUpHit1,
        torpedoUpHit2,
        torpedoUpHit3,
        torpedoUpHit4
        };

        torpedoDownMissSprites = new Sprite[5]
        {
        torpedoDownMiss0,
        torpedoDownMiss1,
        torpedoDownMiss2,
        torpedoDownMiss3,
        torpedoDownMiss4
        };

        torpedoDownHitSprites = new Sprite[5]
        {
        torpedoDownHit0,
        torpedoDownHit1,
        torpedoDownHit2,
        torpedoDownHit3,
        torpedoDownHit4
        };

        if (torpedoOverlayImage != null)
        {
            torpedoOverlayImage.sprite = null;
            torpedoOverlayImage.gameObject.SetActive(false);

            Color c = torpedoOverlayImage.color;
            c.a = 0.5f;
            torpedoOverlayImage.color = c;

            torpedoOverlayImage.raycastTarget = false;
        }

        if (bombOverlayImage != null)
        {
            bombOverlayImage.sprite = null;
            bombOverlayImage.gameObject.SetActive(false);

            Color c = bombOverlayImage.color;
            c.a = 0f;
            bombOverlayImage.color = c;

            bombOverlayImage.raycastTarget = false;
        }
    }

    public void Init(
        Vector2Int c,
        Action<Vector2Int> onClick,
        Action<Vector2Int> onHoverEnter = null,
        Action<Vector2Int> onHoverExit = null)
    {
        coord = c;

        this.onClick = onClick;
        this.onHoverEnter = onHoverEnter;
        this.onHoverExit = onHoverExit;

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => this.onClick?.Invoke(coord));
        }
    }

    public void ApplyRenderState(RenderState rs)
    {
        if (img == null) return;

        switch (rs)
        {
            case RenderState.Sea:
                img.sprite = seaSprite;
                break;

            case RenderState.GunMiss:
                img.sprite = gunMissSprite;
                break;

            case RenderState.GunHit:
                img.sprite = gunHitSprite;
                break;

            case RenderState.ScoutShip:
                img.sprite = scoutShipSprite;
                break;

            case RenderState.ScoutDamagedShip:
                img.sprite = scoutDamagedShipSprite;
                break;

            case RenderState.ScoutEmpty:
                img.sprite = scoutEmptySprite;
                break;

            //case RenderState.BombArea:
            //    img.sprite = bombAreaSprite;
            //    break;

            //case RenderState.BombHit:
            //    img.sprite = bombHitSprite;
            //    break;

            //case RenderState.BombAreaHit:
            //    img.sprite = bombAreaHitSprite;
            //    break;

            default:
                img.sprite = seaSprite;
                break;
        }

        img.color = Color.white;
    }

    private void ApplySprite(Sprite sprite, Color color)
    {
        img.sprite = sprite;
        img.color = color;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
    }

    public void SetPreview(Color color)
    {
        // 不再直接染主图，统一走 preview overlay
        if (previewOverlayImage == null) return;

        previewOverlayImage.sprite = null; // 没有贴图时可以先只显示纯色，也可以直接不用这个接口
        previewOverlayImage.gameObject.SetActive(true);

        Color c = previewOverlayImage.color;
        c.r = color.r;
        c.g = color.g;
        c.b = color.b;
        c.a = color.a;
        previewOverlayImage.color = c;
    }

    public void ClearPreview()
    {
        // 1. 清新的 preview overlay
        if (previewOverlayImage != null)
        {
            previewOverlayImage.sprite = null;
            previewOverlayImage.gameObject.SetActive(false);

            Color pc = previewOverlayImage.color;
            pc.a = 0f;
            previewOverlayImage.color = pc;
        }

        // 2. 兜底：把旧主图颜色恢复
        if (img != null)
        {
            Color c = img.color;
            c.r = 1f;
            c.g = 1f;
            c.b = 1f;
            c.a = 1f;
            img.color = c;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnter?.Invoke(coord);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke(coord);
    }

    public void ClearTorpedoOverlay()
    {
        if (torpedoOverlayImage == null) return;

        torpedoOverlayImage.sprite = null;
        torpedoOverlayImage.gameObject.SetActive(false);
    }

    public void SetTorpedoOverlay(Dir4 dir, int index, bool isHitLine)
    {
        if (torpedoOverlayImage == null) return;
        if (index < 0 || index >= 5) return;

        Sprite sprite = GetTorpedoSprite(dir, index, isHitLine);

        if (sprite == null)
        {
            Debug.LogWarning($"[CellView] Torpedo sprite missing. dir={dir}, index={index}, isHit={isHitLine}");
            torpedoOverlayImage.sprite = null;
            torpedoOverlayImage.gameObject.SetActive(false);
            return;
        }

        torpedoOverlayImage.sprite = sprite;

        Color c = torpedoOverlayImage.color;
        c.a = 0.5f;
        torpedoOverlayImage.color = c;

        torpedoOverlayImage.gameObject.SetActive(true);
    }

    private Sprite GetTorpedoSprite(Dir4 dir, int index, bool isHitLine)
    {
        if (index < 0 || index >= 5) return null;

        switch (dir)
        {
            case Dir4.Right:
                return isHitLine ? torpedoRightHitSprites[index] : torpedoRightMissSprites[index];

            case Dir4.Left:
                return isHitLine ? torpedoLeftHitSprites[index] : torpedoLeftMissSprites[index];

            case Dir4.Up:
                return isHitLine ? torpedoUpHitSprites[index] : torpedoUpMissSprites[index];

            case Dir4.Down:
                return isHitLine ? torpedoDownHitSprites[index] : torpedoDownMissSprites[index];

            default:
                return null;
        }
    }

    public void SetPreviewSprite(Sprite sprite, float alpha)
    {
        if (previewOverlayImage == null) return;

        if (sprite == null || alpha <= 0f)
        {
            ClearPreview();
            return;
        }

        previewOverlayImage.sprite = sprite;

        Color c = previewOverlayImage.color;
        c.r = 1f;
        c.g = 1f;
        c.b = 1f;
        c.a = alpha;
        previewOverlayImage.color = c;

        previewOverlayImage.gameObject.SetActive(true);
        previewOverlayImage.transform.SetAsLastSibling();
    }

    public void ClearBombOverlay()
    {
        if (bombOverlayImage == null) return;

        bombOverlayImage.sprite = null;
        bombOverlayImage.gameObject.SetActive(false);
    }

    public void SetBombOverlay(QuadPart part, bool isHit, float alpha = 1f)
    {
        if (bombOverlayImage == null) return;

        Sprite sprite = null;

        if (isHit)
        {
            switch (part)
            {
                case QuadPart.TL: sprite = bombHitTL; break;
                case QuadPart.TR: sprite = bombHitTR; break;
                case QuadPart.BL: sprite = bombHitBL; break;
                case QuadPart.BR: sprite = bombHitBR; break;
            }
        }
        else
        {
            switch (part)
            {
                case QuadPart.TL: sprite = bombMissTL; break;
                case QuadPart.TR: sprite = bombMissTR; break;
                case QuadPart.BL: sprite = bombMissBL; break;
                case QuadPart.BR: sprite = bombMissBR; break;
            }
        }

        if (sprite == null)
        {
            ClearBombOverlay();
            return;
        }

        bombOverlayImage.sprite = sprite;

        Color c = bombOverlayImage.color;
        c.r = 1f;
        c.g = 1f;
        c.b = 1f;
        c.a = alpha;
        bombOverlayImage.color = c;

        bombOverlayImage.gameObject.SetActive(true);
    }
}